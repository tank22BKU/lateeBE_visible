import torch
from datasets import load_dataset
from trl import SFTTrainer 
from peft import LoraConfig
from transformers import (
    AutoModelForCausalLM,
    AutoTokenizer,
    BitsAndBytesConfig,
    TrainingArguments
)
from huggingface_hub import login
import os
import gc
from dotenv import load_dotenv

# ==========================================
# 1. SETUP MÔI TRƯỜNG & BIẾN
# ==========================================

# Load biến môi trường
load_dotenv()
hf_token = os.getenv("HF_TOKEN")
if not hf_token:
    print("Cảnh báo: Không tìm thấy HF_TOKEN. Hãy chắc chắn bạn đã login hoặc set biến môi trường.")
else:
    login(token=hf_token)

# Tên Model
MODEL_NAME = "meta-llama/Meta-Llama-3.1-8B-Instruct" 
NEW_MODEL_NAME = "Llama-3.1-Virtual-Patient-MimicIV_Ver2.0"

# Hàm giải phóng bộ nhớ GPU
def clear_gpu_memory():
    if torch.cuda.is_available():
        torch.cuda.empty_cache()
        torch.cuda.ipc_collect()
    gc.collect()

clear_gpu_memory()

# ==========================================
# 2. LOAD DATASET & CHIA TRAIN/TEST (QUAN TRỌNG)
# ==========================================
print("--> Đang load và chia dữ liệu...")

# Load toàn bộ dữ liệu
full_dataset = load_dataset("json", data_files="train_dataset.jsonl", split="train")

# Chia 80% Train - 20% Test (seed=42 để cố định kết quả chia)
dataset_dict = full_dataset.train_test_split(test_size=0.2, seed=42)

train_dataset = dataset_dict['train']
eval_dataset = dataset_dict['test']

print(f"   + Tổng số mẫu: {len(full_dataset)}")
print(f"   + Train set (80%): {len(train_dataset)}")
print(f"   + Test set (20%): {len(eval_dataset)}")

# ==========================================
# 3. LOAD MODEL & TOKENIZER (QLoRA)
# ==========================================
print("--> Đang load Model & Tokenizer...")

bnb_config = BitsAndBytesConfig(
    load_in_4bit=True,
    bnb_4bit_quant_type="nf4",
    bnb_4bit_compute_dtype=torch.float16,
    llm_int8_enable_fp32_cpu_offload=True
)

model = AutoModelForCausalLM.from_pretrained(
    MODEL_NAME,
    quantization_config=bnb_config,
    device_map="auto",
    # offload_folder="offload_temp", # Bật nếu VRAM thấp
    max_memory={0: "7GiB", "cpu": "32GiB"}
)

tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME, trust_remote_code=True)
tokenizer.pad_token = tokenizer.eos_token 

# ==========================================
# 4. CẤU HÌNH LoRA
# ==========================================
peft_config = LoraConfig(
    r=16,       
    lora_alpha=16,
    lora_dropout=0.05,
    bias="none",
    task_type="CAUSAL_LM",
    target_modules=["q_proj", "k_proj", "v_proj", "o_proj", "gate_proj", "up_proj", "down_proj"] 
)

# ==========================================
# 5. HÀM FORMAT DỮ LIỆU
# ==========================================
def formatting_prompts_func(example):
    output_texts = []
    for message_list in example['messages']:
        text = tokenizer.apply_chat_template(message_list, tokenize=False, add_generation_prompt=False)
        output_texts.append(text)
    return output_texts

# ==========================================
# 6. CẤU HÌNH TRAINING (CÓ VALIDATION)
# ==========================================
training_args = TrainingArguments(
    output_dir="./results",
    
    # --- Cấu hình Hyperparameters ---
    num_train_epochs=3,            
    per_device_train_batch_size=1,  
    gradient_accumulation_steps=8, 
    learning_rate=2e-4,
    fp16=True,                      
    optim="paged_adamw_8bit",       
    gradient_checkpointing=True,    
    
    # --- Cấu hình Đánh giá ---
    eval_strategy="steps",    
    eval_steps=50,                  
    per_device_eval_batch_size=1,   
    
    save_strategy="steps",          
    save_steps=50,
    load_best_model_at_end=True,    
    metric_for_best_model="eval_loss", 
    
    # --- Logging ---
    logging_steps=10,
    report_to="none"                
)

trainer = SFTTrainer(
    model=model,
    train_dataset=train_dataset,
    eval_dataset=eval_dataset,     
    peft_config=peft_config,
    max_seq_length=512,            
    formatting_func=formatting_prompts_func,
    tokenizer=tokenizer,
    args=training_args,
)

# ==========================================
# 7. TRAINING & SAVE
# ==========================================
print("==> Bắt đầu Training...")
train_result = trainer.train() 

print("==> Training kết thúc.")

metrics = train_result.metrics
trainer.log_metrics("train", metrics)
trainer.save_metrics("train", metrics)
trainer.save_state() 

print(f"--> Đang lưu model tốt nhất vào: {NEW_MODEL_NAME}")
trainer.model.save_pretrained(NEW_MODEL_NAME)
tokenizer.save_pretrained(NEW_MODEL_NAME)

print("Done! Có thể load model này để test.")