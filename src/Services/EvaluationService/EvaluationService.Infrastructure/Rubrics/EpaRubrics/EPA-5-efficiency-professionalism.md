## EPA 5 — Interaction Efficiency and Professionalism
**Max Score:** 20 | **Data Source:** vp_conversation_log + ai_reasoning_log

### Scoring Rubric
| Level | Score | Behavioral Indicators |
|-------|-------|-----------------------|
| Excellent | 17–20 | Zero redundancy, clear case presentation, professional communication |
| Good | 13–16 | Minimal repetition, organized presentation |
| Fair | 9–12 | Noticeable redundancy, disorganized |
| Poor | 0–8 | High redundancy, unprofessional language, no structure |

### Metrics
- REDUNDANCY_INDEX = repeated_questions / total_questions (target < 0.10)
- CASE_PRESENTATION_CLARITY: Can summarize in ≤3 sentences after VP interview?
- COMMUNICATION_QUALITY: Appropriate medical terminology without jargon overload