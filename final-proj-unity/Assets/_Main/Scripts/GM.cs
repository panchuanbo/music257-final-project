using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using ProgressBar;

public class GM : MonoBehaviour {

    private const bool kDebugMode = false;
    private const int kSoundOffset = 15;
    private const int kSoundFrequency = 440 * 2;
    private const float kSkateboardBaseSpeed = 0.010f;
    private const float kBalanceThreshold = 0.08f;
    private const float kStartofWorld = -52.38848f;
    private const float kEndOfWorld = 83.0f;
    private const float kEndOfWorldDebug = -40.0f;

    // MARK: - Camera Rig
    public GameObject cameraRig;
    
    // MARK: - Texts
    public Text messageText;
    public Text balanceFeedbackText;

    // MARK: - Progress Bar & Other Metrics
    public GameObject speedRadial;
    public GameObject gameProgress;
    public GameObject grapher;

    // MARK: - Controller
    public GameObject leftController;   // Actual Controller
    public GameObject rightController;  // Skateboard Controller
    public GameObject skateboard;

    // MARK - Management
    private bool gameStarted = false;
    private bool gameFinished = false;
    private SineWaveGenerator generator;
    private SteamVR_TrackedStaticObject skateboardInput;
    private SteamVR_TrackedController userController;

    private float skateboardSpeedLevel = 1;

    // MARK: - Metrics
    private int fallCounter = 0;
    List<float> balanceData = new List<float>();

    // Use this for initialization
    void Start () {
        // UnityEngine.XR.InputTracking.disablePositionalTracking = true;

        this.userController = this.leftController.GetComponent<SteamVR_TrackedController>();
        this.userController.TriggerUnclicked += controllerTriggerClicked;
        this.userController.PadUnclicked += controllerPadClicked;

        this.skateboardInput = rightController.GetComponent<SteamVR_TrackedStaticObject>();

        this.generator = this.gameObject.GetComponent<SineWaveGenerator>();
        this.speedRadial.GetComponent<ProgressRadialBehaviour>().Value = skateboardSpeedLevel * 10;


        this.speedRadial.SetActive(true);
        this.gameProgress.SetActive(false);
    }
	
	// Update is called once per frame
	void Update() {
        moveCamera();
        adjustSoundAndText();
        checkGameEnd();
    }

    void FixedUpdate() {
        grapher.GetComponent<GraphScript>().graph(quaterionToGyro(this.skateboardInput.transform.rotation).y * 10);
    }

    // MARK: - Game Management

    private void moveCamera() {
        if (!gameStarted) return;
        
        Vector3 oldPos = cameraRig.transform.position;
        cameraRig.transform.position = new Vector3(
            oldPos.x + kSkateboardBaseSpeed * skateboardSpeedLevel, 
            oldPos.y, oldPos.z);
    }

    private void adjustSoundAndText() {
        if (skateboardInput.isActiveAndEnabled) {
            Vector3 gyro = quaterionToGyro(this.skateboardInput.transform.rotation);
            float rotation = gyro.y;

            if (rotation > 0) {
                this.generator.frequency1 = kSoundFrequency + kSoundOffset * rotation;
                this.generator.frequency2 = kSoundFrequency;
            } else if (rotation < 0) {
                this.generator.frequency1 = kSoundFrequency;
                this.generator.frequency2 = kSoundFrequency - kSoundOffset * rotation;
            } else {
                this.generator.frequency1 = kSoundFrequency;
                this.generator.frequency2 = kSoundFrequency;
            }

            if (kDebugMode) {
                if (Mathf.Abs(rotation) < kBalanceThreshold) {
                    balanceFeedbackText.text = "Balanced!";
                } else if (rotation > kBalanceThreshold) {
                    if (rotation > kBalanceThreshold * 2) balanceFeedbackText.text = "Very Left";
                    else balanceFeedbackText.text = "Leaning Left";
                } else if (rotation < -kBalanceThreshold) {
                    if (rotation < -kBalanceThreshold * 2) balanceFeedbackText.text = "Very Right";
                    else balanceFeedbackText.text = "Leaning Right";
                }

                if (Mathf.Abs(rotation) >= 0.27) {
                    fallCounter += 1;
                }

                balanceFeedbackText.text += " " + rotation;
            }
        }
    }

    private void checkGameEnd() {
        float worldEndPosition = (kDebugMode) ? kEndOfWorldDebug : kEndOfWorld;

        float offset = cameraRig.transform.position.x - kStartofWorld;
        this.gameProgress.GetComponent<ProgressBarBehaviour>().Value = offset / (worldEndPosition - kStartofWorld) * 100;

        if (cameraRig.transform.position.x > worldEndPosition) {
            messageText.text = "You made it!";
            messageText.enabled = true;
            gameFinished = true;
            gameStarted = false;
        }
    }

    private void playAgain() {
        skateboardSpeedLevel = 1;
        messageText.text = "Press the trigger\nto begin!";
        balanceData.Clear();
        fallCounter = 0;
        StartCoroutine("ResetScene");
    }

    IEnumerator ResetScene() {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Primary");
    }

    // MARK: - Controller Action Handlers

    private void controllerTriggerClicked(object sender, ClickedEventArgs e) {
        if (gameFinished) {
            playAgain();
        } else {
            Debug.Log("Got Clicked");
            if (gameStarted) {
                this.speedRadial.SetActive(true);
                messageText.enabled = true;

                this.generator.stop();
                this.gameProgress.SetActive(false);
            } else {
                this.speedRadial.SetActive(false);
                messageText.enabled = false;

                this.generator.play();
                this.gameProgress.SetActive(true);
            }

            this.gameStarted = !this.gameStarted;
        }

    }

    private void controllerPadClicked(object sender, ClickedEventArgs e) {
        Debug.Log("Clicked: " + e.padY);
        if (e.padY > 0.5f) {
            Debug.Log("INCREMENT");
            skateboardSpeedLevel += 1;
        } else if (e.padY < -0.5f) {
            Debug.Log("DECREMENT");
            skateboardSpeedLevel -= 1;
        }

        if (skateboardSpeedLevel > 10) skateboardSpeedLevel = 10;
        if (skateboardSpeedLevel < 1) skateboardSpeedLevel = 1;

        this.speedRadial.GetComponent<ProgressRadialBehaviour>().Value = skateboardSpeedLevel * 10;
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
