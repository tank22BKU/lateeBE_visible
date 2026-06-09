## EPA 1 — Information Gathering
**Max Score:** 20 | **Data Source:** vp_conversation_log

### Scoring Rubric
| Level | Score | Behavioral Indicators |
|-------|-------|-----------------------|
| Excellent | 17–20 | Full OLD CART, proactive RED FLAG screening, open→focused→closed hierarchy |
| Good | 13–16 | Most history covered, 1–2 OLD CART gaps, partial RED FLAGS |
| Fair | 9–12 | Present illness only, no systematic approach |
| Poor | 0–8 | Random questions, misses RED FLAGS, premature interview closure |

### Quantitative Metrics
- OLD_CART_COVERAGE = (elements_addressed / 7) × 100%
- RED_FLAG_RATE = (red_flags_asked / applicable) × 100%
- REDUNDANCY_INDEX = repeated_questions / total (lower = better)

### Safety Indicators
- CRITICAL MISS (-3): No alcohol question when abdominal pain + nausea present
- CRITICAL MISS (-3): No gallstone history for epigastric pain
- RED FLAG OMISSION: Triggered if zero RED FLAG questions in transcript