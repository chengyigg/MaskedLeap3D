using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音效源")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("音效文件（把对应音频拖进来）")]
    public AudioClip 跳跃;
    public AudioClip 二段跳;
    public AudioClip 落地;
    public AudioClip 冲刺;
    public AudioClip 金币;
    public AudioClip 弹簧;
    public AudioClip 踩敌人;
    public AudioClip 受伤;
    public AudioClip 存档点;
    public AudioClip 重生;
    public AudioClip 通关;
    public AudioClip 按钮点击;
    public AudioClip 按钮悬停;
    public AudioClip 暂停打开;
    public AudioClip 暂停关闭;
    public AudioClip 胜利号角;

    [Header("背景音乐")]
    public AudioClip 关卡BGM;
    [Range(0f, 1f)] public float bgmVolume = 0.4f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        // 【删掉 DontDestroyOnLoad(gameObject);】
    }

    void Start()
    {
        if (bgmSource != null && 关卡BGM != null)
        {
            bgmSource.clip = 关卡BGM;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayJump() => PlaySFX(跳跃);
    public void PlayDoubleJump() => PlaySFX(二段跳);
    public void PlayLand() => PlaySFX(落地);
    public void PlayDash() => PlaySFX(冲刺);
    public void PlayCoin() => PlaySFX(金币);
    public void PlaySpring() => PlaySFX(弹簧);
    public void PlayEnemyStomp() => PlaySFX(踩敌人);
    public void PlayHurt() => PlaySFX(受伤);
    public void PlayCheckpoint() => PlaySFX(存档点);
    public void PlayRespawn() => PlaySFX(重生);
    public void PlayWinTrigger() => PlaySFX(通关);
    public void PlayButtonClick() => PlaySFX(按钮点击);
    public void PlayButtonHover() => PlaySFX(按钮悬停);
    public void PlayPauseOpen() => PlaySFX(暂停打开);
    public void PlayPauseClose() => PlaySFX(暂停关闭);
    public void PlayWinFanfare() => PlaySFX(胜利号角);
}