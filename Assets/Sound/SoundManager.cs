using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Audio Clips List (Auto Loaded)")]
    public List<AudioClip> audioClips = new List<AudioClip>();

    // 딕셔너리로 빠른 검색
    private Dictionary<string, AudioClip> clipDict = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환해도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 딕셔너리 초기화
        foreach (var clip in audioClips)
        {
            if (clip != null && !clipDict.ContainsKey(clip.name))
            {
                clipDict.Add(clip.name, clip);
            }
        }
        
        // 오디오 소스가 연결 안 되어 있으면 자동 생성
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_Source");
            bgmObj.transform.parent = transform;
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.parent = transform;
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }
    }

    public void PlayBGM(string name)
    {
        if (clipDict.TryGetValue(name, out AudioClip clip))
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return; 
            StartCoroutine(FadeBGM(clip));
        }
        else
        {
            Debug.LogWarning($"SoundManager: BGM '{name}'을(를) 찾을 수 없습니다! Audio Clips 리스트를 확인하세요.");
        }
    }

    IEnumerator FadeBGM(AudioClip newClip)
    {
        float fadeDuration = 0.5f;
        float startVolume = bgmSource.volume;

        // Fade Out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 0f;
        bgmSource.Stop();

        // Change Clip
        bgmSource.clip = newClip;
        bgmSource.Play();

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        bgmSource.volume = 1f;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string name, float volume = 1.0f)
    {
        if (clipDict.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f); // 피치 랜덤 (0.9 ~ 1.1)
            sfxSource.PlayOneShot(clip, volume);
            // 다음 소리를 위해 피치 초기화는 선택사항이지만 OneShot은 영향 안받음
            // 하지만 연속 재생 시 source.pitch가 계속 바뀌어 있을 수 있으므로 reset 추천
            // sfxSource.pitch = 1.0f; (PlayOneShot은 source pitch 영향을 받으므로 랜덤하게 두면 됨)
        }
        else
        {
            // Debug.LogWarning($"[SoundManager] SFX Not Found: {name}"); 
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Load All Sounds From Folder")]
    public void LoadAllSounds()
    {
        audioClips.Clear();
        // Assets/Sound 폴더 내의 모든 wav, mp3, ogg 검색
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Sound" });
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip != null)
            {
                audioClips.Add(clip);
            }
        }
        Debug.Log($"[SoundManager] {audioClips.Count} AudioClips Loaded from Assets/Sound");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
