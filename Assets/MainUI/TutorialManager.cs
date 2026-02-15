using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public string[] tutorialSteps;
    public float typingSpeed = 0.1f;
    public GameObject[] zombies; // 좀비 오브젝트들 (여러 마리 지원)

    private int currentIndex = 0;
    private bool isTyping = false;

    void Start()
    {
        // 좀비들을 보이되 얼려놓기
        FreezeZombies(true);

        if (tutorialSteps.Length > 0)
        {
            StartCoroutine(TypeSentence(tutorialSteps[currentIndex]));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                tutorialText.text = tutorialSteps[currentIndex];
                isTyping = false;
            }
            else
            {
                NextStep();
            }
        }
    }

    void NextStep()
    {
        currentIndex++;

        if (currentIndex < tutorialSteps.Length)
        {
            StartCoroutine(TypeSentence(tutorialSteps[currentIndex]));
        }
        else
        {
            tutorialText.text = "";

            // 좀비 움직임 시작
            FreezeZombies(false);

            StartCoroutine(CheckEnemiesDead());
        }
    }

    void FreezeZombies(bool freeze)
    {
        foreach (GameObject z in zombies)
        {
            if (z == null) continue;
            z.SetActive(true);
            zombie zScript = z.GetComponent<zombie>();
            if (zScript != null)
            {
                zScript.SetFrozen(freeze);
            }
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        tutorialText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            tutorialText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    IEnumerator CheckEnemiesDead()
    {
        Debug.Log("[Tutorial] CheckEnemiesDead 시작! 좀비 수: " + zombies.Length);
        while (true)
        {
            bool allDead = true;
            foreach (GameObject z in zombies)
            {
                if (z != null && z.activeInHierarchy)
                {
                    allDead = false;
                    break;
                }
            }

            if (allDead)
            {
                Debug.Log("[Tutorial] 모든 좀비 처치! MainUI로 이동");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}
