using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public string[] tutorialSteps;
    public float typingSpeed = 0.1f;
    public GameObject zombie;

    private int currentIndex = 0;
    private bool isTyping = false;

    void Start()
    {
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
            zombie.SetActive(true);

            StartCoroutine(CheckEnemiesDead());
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
        while (true)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");

            if (enemies.Length == 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainUI");
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
}