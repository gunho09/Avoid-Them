using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BGMControl : MonoBehaviour
{
    public AudioMixer mixer;    // MyMixer 연결
    public Slider bgmSlider;    // 씬에 있는 슬라이더 연결
    public AudioSource bgmSource; // BGMManager에 붙은 AudioSource 연결

    void Start()
    {
        // 1. 저장된 볼륨값 가져오기 (없으면 0.75)
        float savedVol = PlayerPrefs.GetFloat("BGM_Save", 0.75f);

        // 2. 슬라이더가 이 씬에 있다면 세팅
        if (bgmSlider != null)
        {
            bgmSlider.value = savedVol;
            bgmSlider.onValueChanged.AddListener(SetVolume);
        }

        // 3. 씬이 시작되자마자 현재 저장된 볼륨으로 소리 키우기
        SetVolume(savedVol);
    }

    public void SetVolume(float value)
    {
        // 볼륨 계산 (-40 ~ 0 dB)
        float db = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
        if (value <= 0.001f) db = -80f;

        // 믹서에 적용
        mixer.SetFloat("BGMVol", db);

        // 볼륨값 저장 (다음 씬에서 쓰기 위해)
        PlayerPrefs.SetFloat("BGM_Save", value);
    }
}