using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : SingletonMono<AudioSystem>
{
    [Header("System Audio")]
    public AudioSource systemAudioSource;
    public AudioClip openClip;
    public AudioClip closeClip;
    public AudioClip tickClip;

    public void SystemAudioPlay(AudioClip clip, float volume)
    {
        systemAudioSource.loop = false;
        systemAudioSource.clip = clip;
        systemAudioSource.volume = volume;
        systemAudioSource.Play();
    }


    [Header("Scene Audio")]
    public GameObject sceneAudio;
    private int poolSize = GeneratorDataValue.audios.Length;
    private Queue<AudioSource> sceneAudioSourcePool = new();
    private bool changeList = false;

    public AudioClip gushengClip;
    public AudioClip zhongshengClip;
    public AudioClip qinshengClip;
    public AudioClip dishengClip;
    public AudioClip jimingClip;
    public AudioClip quanfeiClip;
    public AudioClip yuantiClip;
    public AudioClip matiClip;
    public AudioClip wutiClip;
    public AudioClip niaomingClip;
    public AudioClip yanmingClip;
    public AudioClip chanmingClip;
    public AudioClip chongshengClip;
    public Dictionary<string, AudioClip> sceneAudioDictionary = new();
    private List<AudioClip> sceneClips = new();

    private void PlaySceneClip(AudioClip clip)
    {
        if (sceneAudioSourcePool.Count == 0)
        {
            Debug.LogError("empty audio source pool.");
            return;
        }

        AudioSource source = sceneAudioSourcePool.Dequeue();
        Coroutine playClip = StartCoroutine(DelayLoopPlay(source, clip));
        StartCoroutine(RecycleAudioSource(source, playClip));
    }

    private IEnumerator DelayLoopPlay(AudioSource source, AudioClip clip)
    {
        while (true) 
        {
            source.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length + 5);
        }
    }

    private IEnumerator RecycleAudioSource(AudioSource source, Coroutine playClip)
    {
        while (!changeList)
        {
            yield return null;
        }

        StopCoroutine(playClip);
        source.Stop();
        sceneAudioSourcePool.Enqueue(source);
    }

    private IEnumerator RecycleAudioSource()
    {
        // recycle audio source
        changeList = true;
        yield return new WaitForSeconds(3);

        // set for new clipList
        changeList = false;
        if (sceneAudioSourcePool.Count == poolSize)
        {
            foreach (AudioClip clip in sceneClips)
            {
                PlaySceneClip(clip);
            }
        }
        else
        {
            Debug.LogError("fail to recycle scene audio source");
        }
    }

    public void SceneAudioPlay(List<AudioClip> clips)
    {
        sceneClips = clips;

        // delay for recycle source
        StartCoroutine(RecycleAudioSource());
    }


    [Header("BGM Audio")]
    public AudioSource bgmAudioSource;
    public AudioClip ninjingClip;
    public AudioClip haomaiClip;
    public AudioClip qinkuaiClip;
    public AudioClip gujiClip;
    public AudioClip cangliangClip;
    public Dictionary<string, AudioClip> bgmAudioDictionary = new();

    public void BGMAudioPlay(AudioClip clip)
    {
        if (sceneClips.Contains(qinshengClip) || sceneClips.Contains(dishengClip))
        {
            bgmAudioSource.Stop();
            return;
        }      

        bgmAudioSource.clip = clip;
        bgmAudioSource.Play();
    }


    private void Start()
    {
        // set scene audio dict
        sceneAudioDictionary["¹ÄÉù"] = gushengClip;
        sceneAudioDictionary["ÖÓÉù"] = zhongshengClip;
        sceneAudioDictionary["ÇÙÉù"] = qinshengClip;
        sceneAudioDictionary["µÑÉù"] = dishengClip;
        sceneAudioDictionary["¼¦Ãù"] = jimingClip;
        sceneAudioDictionary["È®·Í"] = quanfeiClip;
        sceneAudioDictionary["Ô³Ìä"] = yuantiClip;
        sceneAudioDictionary["ÂíÌãÉù"] = matiClip;
        sceneAudioDictionary["ÎÚÌä"] = wutiClip;
        sceneAudioDictionary["ÄñÃù"] = niaomingClip;
        sceneAudioDictionary["ÑãÃù"] = yanmingClip;
        sceneAudioDictionary["²õÃù"] = chanmingClip;
        sceneAudioDictionary["³æÉù"] = chongshengClip;

        // set bgm audio dict
        bgmAudioDictionary["Äþ¾²Ìñµ­"] = ninjingClip;
        bgmAudioDictionary["ºÀÂõ¿õ´ï"] = haomaiClip;
        bgmAudioDictionary["Çá¿ìÃ÷ÀÊ"] = qinkuaiClip;
        bgmAudioDictionary["¹Â¼Åã°âê"] = gujiClip;
        bgmAudioDictionary["²ÔÁ¹±¯×³"] = cangliangClip;

        bgmAudioSource.loop = true;
        bgmAudioSource.volume = 0.2f;

        // init scene audio source pool
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = sceneAudio.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0.6f;
            sceneAudioSourcePool.Enqueue(source);
        }
    }
}
