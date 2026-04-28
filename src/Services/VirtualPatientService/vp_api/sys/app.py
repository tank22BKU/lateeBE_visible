import streamlit as st
import torch
import os
import gc
import csv
import time
import json
from transformers import AutoModelForCausalLM, AutoTokenizer, BitsAndBytesConfig
from peft import PeftModel
from huggingface_hub import login
from dotenv import load_dotenv

# ==========================================
# 1. CẤU HÌNH & HẰNG SỐ
# ==========================================
load_dotenv()
hf_token = os.getenv("HUGGINGFACE_TOKEN")
if hf_token: login(token=hf_token)

st.set_page_config(page_title="Latee - Virtual Patient System", page_icon="🩺", layout="wide")

BASE_MODEL = "meta-llama/Meta-Llama-3.1-8B-Instruct" 
ADAPTER_PATH = "./Llama-3.1-Virtual-Patient-MimicIV_Ver2.0"
PATIENT_AVATAR = "assets/LVP1.jpeg"
DOCTOR_AVATAR = "assets/doctor1.png"
LOG_FILE = "patient_logs.csv"
PROMPTS_FILE = "system_prompts.json"

# ==========================================
# 2. LOAD DATA BỆNH NHÂN
# ==========================================
# Load file cấu hình đã tạo từ generate_configs.py
if not os.path.exists(PROMPTS_FILE):
    st.error(f"Không tìm thấy file '{PROMPTS_FILE}'. Hãy chạy 'generate_configs.py' trước!")
    st.stop()

with open(PROMPTS_FILE, "r", encoding="utf-8") as f:
    PATIENT_DB = json.load(f)

patient_ids = list(PATIENT_DB.keys())

# ==========================================
# 3. HÀM HỖ TRỢ
# ==========================================
def free_memory():
    if torch.cuda.is_available():
        torch.cuda.empty_cache()
        torch.cuda.ipc_collect()
    gc.collect()

def log_interaction(patient_id, question, answer, history_len):
    """Ghi log kèm Patient ID để file evaluate.py biết đường chấm điểm"""
    file_exists = os.path.isfile(LOG_FILE)
    try:
        with open(LOG_FILE, mode='a', newline='', encoding='utf-8') as f:
            writer = csv.writer(f)
            # Nếu file mới, ghi header chuẩn
            if not file_exists:
                writer.writerow(["Timestamp", "Patient_ID", "User_Input", "Model_Response", "History_Length"])
            
            writer.writerow([time.strftime("%Y-%m-%d %H:%M:%S"), patient_id, question, answer, history_len])
    except Exception as e:
        print(f"Lỗi ghi log: {e}")

@st.cache_resource
def load_model():
    print("------------ Đang tải model... -------------")
    free_memory()
    bnb_config = BitsAndBytesConfig(
        load_in_4bit=True, bnb_4bit_quant_type="nf4",
        bnb_4bit_compute_dtype=torch.float16, llm_int8_enable_fp32_cpu_offload=True 
    )
    base_model = AutoModelForCausalLM.from_pretrained(
        BASE_MODEL, quantization_config=bnb_config, device_map="auto", trust_remote_code=True
    )
    tokenizer = AutoTokenizer.from_pretrained(BASE_MODEL)
    tokenizer.pad_token = tokenizer.eos_token
    
    try:
        model = PeftModel.from_pretrained(base_model, ADAPTER_PATH, device_map="auto")
        model = model.merge_and_unload()
    except Exception as e:
        st.warning(f"Không tìm thấy Adapter. Đang chạy Base Model. Lỗi: {e}")
        model = base_model
    model.eval()
    return model, tokenizer

# ==========================================
# 4. KHỞI TẠO & GIAO DIỆN
# ==========================================
try:
    model, tokenizer = load_model()
except Exception as e:
    st.error(f"Lỗi tải model: {e}")
    st.stop()

# --- SIDEBAR: CHỌN BỆNH NHÂN ---
with st.sidebar:
    st.title("Patient Selection")
    
    # Dropdown chọn bệnh nhân
    selected_id = st.selectbox(
        "Choose a Case:", 
        patient_ids,
        format_func=lambda x: f"Case {x} - {PATIENT_DB[x]['name']}"
    )
    
    # Lấy dữ liệu của bệnh nhân được chọn
    current_patient = PATIENT_DB[selected_id]
    SYSTEM_PROMPT = current_patient["system_prompt"]
    INITIAL_GREETING = current_patient["initial_greeting"]
    
    if os.path.exists(PATIENT_AVATAR):
        st.image(PATIENT_AVATAR, caption=f"Patient: {current_patient['name']}", width=250)
    
    st.markdown("---")
    
    # Kiểm tra nếu người dùng đổi bệnh nhân -> Reset hội thoại
    if "current_patient_id" not in st.session_state:
        st.session_state.current_patient_id = selected_id
    
    if st.session_state.current_patient_id != selected_id:
        st.session_state.current_patient_id = selected_id
        st.session_state.messages = [
            {"role": "system", "content": SYSTEM_PROMPT}, 
            {"role": "assistant", "content": INITIAL_GREETING}
        ]
        st.rerun()

    # Nút Reset thủ công
    if st.button("New Conversation", key="reset_btn"):
        st.session_state.messages = [
            {"role": "system", "content": SYSTEM_PROMPT}, 
            {"role": "assistant", "content": INITIAL_GREETING}
        ]
        st.rerun()

# --- CHAT INTERFACE ---
st.title("---- Lavender Teeducation ----")
st.caption(f"Case ID: {selected_id} | Name: {current_patient['name']}")

if "messages" not in st.session_state:
    st.session_state.messages = [
        {"role": "system", "content": SYSTEM_PROMPT}, 
        {"role": "assistant", "content": INITIAL_GREETING}
    ]

# 5.3 Các câu hỏi mẫu
st.markdown("###### Suggested Questions:")
col1, col2, col3, col4 = st.columns(4)
sample_prompt = None
if col1.button("Chief Complaint", use_container_width=True): sample_prompt = "Hello, tell me what brings you here?"
if col2.button("History", use_container_width=True): sample_prompt = "Do you have any past medical conditions?"
if col3.button("Exam", use_container_width=True): sample_prompt = "Does it hurt when I press here?"
if col4.button("Social", use_container_width=True): sample_prompt = "Do you smoke or drink?"

# Hiển thị lịch sử
for message in st.session_state.messages:
    if message["role"] == "system": continue
    role = message["role"]
    avatar = PATIENT_AVATAR if role == "assistant" else DOCTOR_AVATAR
    if not os.path.exists(avatar): avatar = "😷" if role == "assistant" else "👨‍⚕️"
    with st.chat_message(role, avatar=avatar):
        st.markdown(message["content"])

# Xử lý Chat
user_input = sample_prompt if sample_prompt else st.chat_input("Enter your clinical question...")

if user_input:
    # 1. Hiển thị User
    st.session_state.messages.append({"role": "user", "content": user_input})
    with st.chat_message("user", avatar=DOCTOR_AVATAR if os.path.exists(DOCTOR_AVATAR) else "👨‍⚕️"):
        st.markdown(user_input)
    
    # 2. AI Generate
    with st.chat_message("assistant", avatar=PATIENT_AVATAR if os.path.exists(PATIENT_AVATAR) else "😷"):
        with st.spinner("Patient is thinking..."):
            input_ids = tokenizer.apply_chat_template(
                st.session_state.messages, add_generation_prompt=True, return_tensors="pt"
            ).to(model.device)
            
            with torch.no_grad():
                outputs = model.generate(
                    input_ids, max_new_tokens=256, do_sample=True, temperature=0.3, top_p=0.9,
                    repetition_penalty=1.1, pad_token_id=tokenizer.eos_token_id
                )
            response = tokenizer.decode(outputs[0][input_ids.shape[-1]:], skip_special_tokens=True)
            st.markdown(response)
    
    # 3. Save & Log
    st.session_state.messages.append({"role": "assistant", "content": response})
    log_interaction(selected_id, user_input, response, len(st.session_state.messages)) # Ghi log kèm ID
    
    free_memory()
    if sample_prompt: st.rerun()