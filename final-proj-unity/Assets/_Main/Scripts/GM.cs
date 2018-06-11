using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GM : MonoBehaviour {

    private const int kSoundOffset = 10;
    private const int kSoundFrequency = 440;

    // MARK: - Camera Rig
    public GameObject cameraRig;

    // MARK: - Texts
    public GameObject welcomeText;

    // MARK: - Controller
    public GameObject leftController;   // Actual Controller
    public GameObject rightController;  // Skateboard

    // MARK - Management
    private bool gameStarted = false;
    private SineWaveGenerator generator;
    private SteamVR_TrackedObject skateboard;
    private SteamVR_TrackedController controller;

    // Use this for initialization
    void Start () {
        this.controller = this.leftController.GetComponent<SteamVR_TrackedController>();
        this.controller.TriggerClicked += controllerTriggerClicked;

        // UnityEngine.XR.InputTracking.disablePositionalTracking = true;
        this.skateboard = rightController.GetComponent<SteamVR_TrackedObject>();

        this.generator = this.gameObject.GetComponent<SineWaveGenerator>();
    }
	
	// Update is called once per frame
	void Update() {
        moveSkateboard();
        adjustSound();
    }

    // MARK: - Game Management

    private void moveSkateboard() {
        if (!gameStarted) return;
        
        Vector3 oldPos = cameraRig.transform.position;
        cameraRig.transform.position = new Vector3(oldPos.x + 0.010f, oldPos.y, oldPos.z);
    }

    private void adjustSound() {
        if (skateboard.isActiveAndEnabled) {
            Vector3 gyro = quaterionToGyro(this.skateboard.transform.rotation);

            if (gyro.x > 0) {
                this.generator.frequency1 = kSoundFrequency + kSoundOffset * gyro.x;
                this.generator.frequency2 = kSoundFrequency;
            } else if (gyro.x < 0) {
                this.generator.frequency1 = kSoundFrequency;
                this.generator.frequency2 = kSoundFrequency + kSoundOffset * gyro.x;
            } else {
                this.generator.frequency1 = kSoundFrequency;
                this.generator.frequency2 = kSoundFrequency;
            }
        }
    }

    private void controllerTriggerClicked(object sender, ClickedEventArgs e) {
        if (gameStarted) {
            this.welcomeText.active = true;
            this.generator.stop();
        } else {
            this.welcomeText.active = false;
            this.generator.play();
        }

        this.gameStarted = !this.gameStarted;
    }

    // MARK: - Helpers

    /**
     * @param quat The quaternion
     * @return Vector3 containing (pitch, yaw, roll)
     */
    private Vector3 quaterionToGyro(Quaternion quat) {
        float x = quat.x, y = quat.y, z = quat.z, w = quat.w;

        float roll = Mathf.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z);
        float pitch = Mathf.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
        float yaw = Mathf.Asin(2 * x * y + 2 * z * w);

        return new Vector3(pitch, yaw, roll);
    }
}
