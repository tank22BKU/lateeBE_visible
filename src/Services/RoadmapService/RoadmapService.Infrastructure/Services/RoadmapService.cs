using RoadmapService.Domain.Services;

namespace RoadmapService.Infrastructure.Services;

/// <summary>
/// Implementation of Gemini API service
/// </summary>
public class RoadmapService : IRoadmapService
{
    private readonly HuggingFaceDeepSeekClient _client;

    public RoadmapService(HuggingFaceDeepSeekClient client)
    {
        _client = client;
    }

    public async Task<string> GenerateResponseAsync(string historyPractice, string userTarget, int amountOfTime)
    {
        //var result = await _client.ChatAsync(this.BuildPrompt(historyPractice, userTarget, amountOfTime));
        var result = await _client.ChatAsyncVer2(this.BuildUserPrompt(historyPractice, userTarget, amountOfTime));
        return result;
    }

    private string BuildPrompt(string historyPractice, string userTarget, int amountOfTime)
    {
        return $$"""
                                 You are an expert medical learning roadmap generator.

                                 Your task is to generate a personalized medical learning roadmap for a learner.

                                 You MUST follow these rules strictly:

                                 1. Return ONLY valid JSON.
                                 2. Do NOT return markdown. - DO NOT include reasoning in the "reasoning_content" field.
                                 3. Do NOT include explanation outside JSON.
                                 4. The roadmap must be ordered progressively from basic -> advanced.
                                 5. Each roadmap item must:
                                    - focus on ONE learning objective
                                    - be clinically relevant
                                    - be concise but educational
                                 6. Use the user's history_practice to avoid repeating mastered topics.
                                 7. Use the user_target as the final learning objective.
                                 8. Spread the roadmap reasonably across the given amount_of_time.
                                 9. Generate between 3 and 7 roadmap items depending on complexity.

                                 INPUT:

                                 history_practice:
                                 {{{historyPractice}}}

                                 user_target:
                                 {{{userTarget}}}

                                 amount_of_time_days:
                                 {{{amountOfTime}}}

                                 OUTPUT FORMAT:

                                 {
                                     "roadmap": [
                                     {
                                         "order_id": 1,
                                         "recommended_content": "string",
                                         "detailed_explain": "string"
                                     },
                                     {
                                         "order_id": 2,
                                         "recommended_content": "string",
                                         "detailed_explain": "string"
                                     },...
                                     ]
                                 }
                 """;
    }

    private string BuildUserPrompt(string historyPractice, string userTarget, int amountOfTime)
    {
        return $$"""
                 <input>
                   <history_practice>{{historyPractice}}</history_practice>
                   <user_target>{{userTarget}}</user_target>
                   <total_days_available>{{amountOfTime}}</total_days_available>
                 </input>

                 <domain_scope>
                   Draw content ONLY from these abdominal pathology domains:
                   1. Anatomy and pathophysiology of the abdominal cavity
                   2. Core conditions: appendicitis, cholecystitis, bowel obstruction,
                      peptic ulcer perforation, pancreatitis, peritonitis, GI bleeding,
                      hernia, abdominal trauma, mesenteric ischemia, intra-abdominal tumors
                   3. History taking: pain character (SOCRATES), associated symptoms,
                      red flags, relevant past medical history, patient communication skills
                   4. Physical exam: inspection→auscultation→percussion→palpation,
                      Murphy/Rovsing/Psoas/Obturator signs, peritonism assessment
                   5. Diagnostics: labs (CBC/CRP/lipase/lactate), imaging (X-ray/US/CT),
                      scoring systems (Alvarado, Ranson)
                   6. Management: triage, conservative vs surgical decision,
                      fluid resuscitation, antibiotic selection, operative indications
                 </domain_scope>

                 <task>
                   Generate between 3 and 5 roadmap items.

                   Ordering rule:
                   - Progress strictly: Foundation → Assessment Skills → Diagnosis → Management.
                   - Skip any topic already mastered in history_practice.
                   - Each item covers exactly ONE learning objective.
                   - detailed_explain: ≤150 words — include WHY clinically important + HOW to study it.

                   Time allocation rule (CRITICAL):
                   - Assign amount_of_time_days to each item based on topic complexity:
                       * Foundation / anatomy topics     → 1–2 days  (lower bound)
                       * Clinical skills / history taking → 2–3 days  (medium)
                       * Diagnostics / scoring systems    → 2–4 days  (medium-high)
                       * Complex management decisions     → 3–5 days  (upper bound)
                   - The SUM of all item amount_of_time_days MUST equal exactly {{amountOfTime}}.
                   - Distribute remaining days to the most complex item if needed to satisfy the sum.
                 </task>

                 <output_schema>
                 {
                    "roadmap_title": "<Roadmap Title>",
                    "goal": "<overall competency learner should achieve after completing the roadmap>",
                    "total_days": {{amountOfTime}},
                    "roadmap": [
                     {
                       "order_id": 1,
                       "recommended_content": "<topic name>",
                       "detailed_explain": "<why clinically important + how to study>",
                       "amount_of_time_days": <integer, days allocated to this item>
                     }, ....
                   ]
                 }
                 </output_schema>
                 <output_requirements>
                     - Return ONLY valid JSON.
                     - Do not include markdown.
                     - Do not wrap JSON inside code fences.
                 </output_requirements>

                 <constraint_check>
                   Before returning, verify: sum of all roadmap[*].amount_of_time_days == {{amountOfTime}}.
                   If not equal, redistribute days until the sum matches exactly.
                 </constraint_check>
                 """;
    }
}