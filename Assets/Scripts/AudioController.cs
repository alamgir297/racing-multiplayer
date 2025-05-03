using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour {

    private float _initialVolume;
    [SerializeField] private float _fadeDuration = 1f;
    private AudioSource _carAudio;

    private Coroutine _fadeRoutine;

    [SerializeField] private AudioClip _engineStart;
    [SerializeField] private AudioClip _engineIdle;
    [SerializeField] private AudioClip _engineRunning;
    [SerializeField] private AudioClip _car_drift;

    private void Awake() {
        _carAudio = GetComponent<AudioSource>();
        _initialVolume = 0.8f;
    }

    private void CrossFadeTo(AudioClip newClip, float duration) {
        if (_carAudio.clip == newClip) return;

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeToClip(newClip, _fadeDuration));
    }

    private void PlayOnce(AudioClip audio) {
        _carAudio.PlayOneShot(audio);
    }

    public void PlayeEnagineStartSound() {
        PlayOnce(_engineStart);
    }

    public void PlayIdleWithDelay(float delay) {
        _carAudio.clip = _engineIdle;
        _carAudio.loop = true;
        _carAudio.PlayDelayed(delay);
    }
    public void PlayEngineIdleSound() {
        CrossFadeTo(_engineRunning, _fadeDuration);
    }
    public void PlayeEngineRunningSound() {
        CrossFadeTo(_engineIdle, _fadeDuration);
    }

    public void PlayDriftSound() {
        PlayOnce(_car_drift);
        Volume(_carAudio, 0.5f);
    }
    private void Volume(AudioSource source, float volume) {
        source.volume = volume;
    }
    

    IEnumerator FadeToClip(AudioClip newClip, float duration) {
        
        //fade out
        for(float t=0; t<duration; t += Time.deltaTime) {
            Volume(_carAudio, Mathf.Lerp(_initialVolume, .5f, t / duration));
            yield return null;
        }

        Volume(_carAudio, .5f);
        _carAudio.loop = true;
        _carAudio.clip = newClip;
        _carAudio.Play();

        //fade in
        for(float t=0; t<duration; t += Time.deltaTime) {
            Volume(_carAudio, Mathf.Lerp(.5f, _initialVolume, t / duration));

            //_carAudio.volume = Mathf.Lerp(0f, _initialVolume, t / duration);
            yield return null;
        }

        Volume(_carAudio, _initialVolume);
    }
}
