using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instnace;

    [SerializeField] private AudioSource audioSourceObject;

    private void Awake() {
        if (instnace == null)
            instnace = this;
    }

    public void PlaySoundAtPosition(AudioClip clip, Vector3 spawnPosition, float volume) {
        AudioSource audioSource = Instantiate(audioSourceObject, spawnPosition, Quaternion.identity);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        float clipLenght = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLenght);
    }

    public void PlayRandomSoundAtPosition(AudioClip[] clips, Vector3 spawnPosition, float volume) {
        int randomIndex = Random.Range(0, clips.Length);
        AudioSource audioSource = Instantiate(audioSourceObject, spawnPosition, Quaternion.identity);
        audioSource.clip = clips[randomIndex];
        audioSource.volume = volume;
        audioSource.Play();
        float clipLenght = audioSource.clip.length;
        Destroy(audioSource.gameObject, clipLenght);
    }
}
