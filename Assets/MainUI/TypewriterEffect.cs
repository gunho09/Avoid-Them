using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText; // TMP 오브젝트 연결
    public string[] tutorialSteps;       // 인스펙터에서 문장 적기
    public float typingSpeed = 0.1f;    // 타이핑 속도
    public GameObject zombie;

    private int currentIndex = 0;
    private bool isTyping = false;       // 현재 타이핑 중인지 확인

    void Start()
    {
        if (tutorialSteps.Length > 0)
        {
            StartCoroutine(TypeSentence(tutorialSteps[currentIndex]));
        }
    }

    void Update()
    {
        // 마우스 좌클릭을 눌렀을 때
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // 1. 타이핑 중이면? -> 한 번에 문장 다 보여주기 (스킵 기능)
                StopAllCoroutines();
                tutorialText.text = tutorialSteps[currentIndex];
                isTyping = false;
            }
            else
            {
                // 2. 타이핑이 끝난 상태면? -> 다음 문장으로 넘어가기
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
             
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        tutorialText.text = ""; // 일단 비우고

        foreach (char letter in sentence.ToCharArray())
        {
            tutorialText.text += letter; // 한 글자씩 추가
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }
}