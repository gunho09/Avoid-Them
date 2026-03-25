using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BGMControl : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider bgmSlider;
    public AudioSource bgmSource;

    void Start()
    {
        float savedVol = PlayerPrefs.GetFloat("BGM_Save", 0.75f);

        if (bgmSlider != null)
        {
            bgmSlider.value = savedVol;
            bgmSlider.onValueChanged.AddListener(SetVolume);
        }

        SetVolume(savedVol);
    }

    void OnDestroy()
    {
        // 리스너 제거 (중복 등록 / MissingReferenceException 방지)
        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        // 무음 기준값 0.001f 으로 통일
        float db = value <= 0.001f ? -80f : Mathf.Log10(value) * 20f;

        // null 체크 후 믹서에 적용
        if (mixer != null)
            mixer.SetFloat("BGMVol", db);
        else
            Debug.LogWarning("BGMControl: AudioMixer가 연결되지 않았습니다.");

        // AudioSource 볼륨 직접 적용 (믹서 없이도 동작하도록)
        if (bgmSource != null)
            bgmSource.volume = value;

        // 볼륨값 저장 + 즉시 디스크에 기록 (크래시 대비)
        PlayerPrefs.SetFloat("BGM_Save", value);
        PlayerPrefs.Save();
    }
}