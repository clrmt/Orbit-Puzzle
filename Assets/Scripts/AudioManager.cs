using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    public static AudioManager instance { get; private set; }

    private List<AudioClip>[] musicBoxSounds; // 오디오 데이터
    private List<AudioSource> audioSources; // 컴포넌트
    private int audioSourceMaxIndex;
    private int audioSourceIndex;

    public AudioMixer audioMixer;
    private AudioMixerGroup audioMixerGroup;

    public AudioClip endingMusic;
    public AudioClip dragSound;
    public AudioClip dropSound;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
            return;
        }

    }

    public void initialize() {

        gameObject.SetActive(true);

        audioSourceMaxIndex = 32;
        audioSourceIndex = 0;

        // 오디오 클립 로드
        LoadAudioClips();

        audioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];

        // 오디오 소스 초기화
        audioSources = new List<AudioSource>();
        for (int i = 0; i < audioSourceMaxIndex; i++) {
            AudioSource _as = gameObject.AddComponent<AudioSource>();
            audioSources.Add(_as);
            _as.outputAudioMixerGroup = audioMixerGroup;
        }
    }

    private void LoadAudioClips() {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sounds");

        musicBoxSounds = new List<AudioClip>[128];
        for (int i = 0; i < 128; i++) {
            musicBoxSounds[i] = new List<AudioClip>();
        }

        bool done = false;
        foreach (AudioClip clip in clips) {
            string pitchName = clip.name;
            int lastUnderscoreIndex = pitchName.LastIndexOf('_');
            if (lastUnderscoreIndex != -1) {
                pitchName = pitchName.Substring(lastUnderscoreIndex + 1);
            }

            int pitch = 0;
            if (pitchName[0] == 'A') {
                pitch = 21;
            } else if (pitchName[0] == 'B') {
                pitch = 23;
            } else if (pitchName[0] == 'C') {
                pitch = 12;
            } else if (pitchName[0] == 'D') {
                pitch = 14;
            } else if (pitchName[0] == 'E') {
                pitch = 16;
            } else if (pitchName[0] == 'F') {
                pitch = 17;
            } else if (pitchName[0] == 'G') {
                pitch = 19;
            }

            if (pitchName[1] == 'b') { // 플랫이나 샵이 없음
                pitch--;
                pitch += ((int)(pitchName[2] - '0')) * 12;
            } else if (pitchName[1] == '#') {
                pitch++;
                pitch += ((int)(pitchName[2] - '0')) * 12;
            } else {
                pitch += ((int)(pitchName[1] - '0')) * 12;
            }

            if (pitch >= 0 && pitch <= 128) {
                musicBoxSounds[pitch + 12].Add(clip);
                done = true;
            }

        }

        if (!done) {
            Debug.LogError("No audio clips found in Resources/Sounds");
        }
    }

    private AudioSource GetAvailableAudioSource() {

        audioSourceIndex++;
        if (audioSourceIndex >= audioSourceMaxIndex) {
            audioSourceIndex = 0;
        }
        return audioSources[audioSourceIndex];

        //foreach (AudioSource source in audioSources) {
        //    if (!source.isPlaying) {
        //        return source;
        //    }
        //}
        ////return audioSources[0];
        //return null;
    }

    private AudioSource GetAvailableAudioSource(int pitch) {
        return audioSources[pitch];
    }

    public void playNote(int pitch, int index) {

    }

    public void playMusic() {
        audioSources[0].pitch = 1.0f;
        audioSources[0].clip = endingMusic;
        audioSources[0].volume = 0.5f;
        audioSources[0].Play();
    }
    public void stopMusic() {
        for (int i = 0; i < audioSourceMaxIndex; i++) {
            audioSources[i].Stop();
        }
    }
    public void playDragSound() {
        audioSources[0].pitch = 1.0f;
        audioSources[0].clip = dragSound;
        audioSources[0].volume = 0.5f;
        audioSources[0].Play();
    }
    public void playDropSound() {
        audioSources[0].pitch = 1.0f;
        audioSources[0].clip = dropSound;
        audioSources[0].volume = 1.0f;
        audioSources[0].Play();
    }
    public void playNote(int pitch) {
        if (pitch <= 0 || pitch >= 128) {
            Debug.LogWarning("pitch값이 범위를 넘어갑니다: " + pitch.ToString());
        }
        int closePitch = -1;
        for (int i = 1; i < 128; i++) { // 일부러 같은 음을 안쓰기?

            if (pitch + i < 128) {
                if (i < 2 && UnityEngine.Random.value < 0.4f) { // 동일한 pitch 사운드는 50% 확률로 실패 - 사운드에 조금 랜덤 다양성 효과를 줌?
                    continue;
                }
                if (musicBoxSounds[pitch + i].Count > 0) {
                    closePitch = pitch + i;
                    break;
                }
            }
            if (pitch - i >= 0) {
                if (i < 2 && UnityEngine.Random.value < 0.4f) {
                    continue;
                }
                if (musicBoxSounds[pitch - i].Count > 0) {
                    closePitch = pitch - i;
                    break;
                }
            }
            if (pitch + i >= 128 && pitch - i <= 0) {
                break;
            }
        }
        if (closePitch >= 0 && closePitch < 128) {
            System.Random random = new System.Random();
            int randomIndex = random.Next(musicBoxSounds[closePitch].Count);
            AudioClip randomClip = musicBoxSounds[closePitch][randomIndex];
            AudioSource source = GetAvailableAudioSource(); // pitch 인자로 넣으면 같은 음당 같은 source 사용
            if (source == null) {
                return;
            }



            source.pitch = Mathf.Pow(1.0594630943593f, (float)(pitch - closePitch));
            source.clip = randomClip;

            float v1 = 1.5f; // 클 수록 저음을 더 낮게
            float minf = Mathf.Pow(v1, 0.0f);
            float maxf = Mathf.Pow(v1, 1.0f);
            float val = Mathf.Pow(v1, Mathf.Clamp01((float)pitch / 88f));
            source.volume = (val - minf) / (maxf - minf);
            source.Play();
        } else {
            Debug.LogWarning("pitch값이 이상함..." + closePitch.ToString());
        }
    }

}
