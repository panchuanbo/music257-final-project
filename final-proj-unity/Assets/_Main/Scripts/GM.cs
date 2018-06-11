using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GM : MonoBehaviour {

    private const int kSoundOffset = 15;
    private const int kSoundFrequency = 440;
    private const float kSkateboardBaseSpeed = 0.010f;
    private const float kBalanceThreshold = 0.08f;
    private const float kEndOfWorld = -40.0f;//83.0f;

    // MARK: - Camera Rig
    public GameObject cameraRig;

    // MARK: - Texts
    public GameObject welcomeText;
    public Text balanceFeedbackText;

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
    }
	
	// Update is called once per frame
	void Update() {
        updateSkateboard();
        moveWorld();
        adjustSoundAndText();
        checkGameEnd();
    }

    void FixedUpdate() {
        balanceData.Add(quaterionToGyro(this.skateboardInput.transform.rotation).y);
    }

    // MARK: - Game Management

    private void updateSkateboard() {
        //this.welcomeText.GetComponent<TextMesh>().text = "" + this.skateboardInput.transform.rotation.eulerAngles;

        //Vector3 rot = this.rightController.transform.rotation.eulerAngles;
        //this.skateboard.transform.rotation = Quaternion.Euler(rot.x - 90, rot.y, rot.z);
    }

    private void moveWorld() {
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

    private void checkGameEnd() {
        if (cameraRig.transform.position.x > kEndOfWorld) {
            welcomeText.GetComponent<TextMesh>().text = "You made it!";
            welcomeText.SetActive(true);
            gameFinished = true;
            gameStarted = false;
        }
    }

    private void playAgain() {
        welcomeText.GetComponent<TextMesh>().text = "Press the trigger\nto begin!";
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
            if (gameStarted) {
                this.welcomeText.SetActive(true);
                this.generator.stop();
            } else {
                this.welcomeText.SetActive(false);
                this.generator.play();
            }

            this.gameStarted = !this.gameStarted;
        }

    }

    private void controllerPadClicked(object sender, ClickedEventArgs e) {
        if (e.padY > 0.7f) {
            skateboardSpeedLevel += 1;
        } else if (e.padY < -0.7f) {
            skateboardSpeedLevel -= 1;
        }

        if (skateboardSpeedLevel > 10) skateboardSpeedLevel = 10;
        if (skateboardSpeedLevel < 1) skateboardSpeedLevel = 1;
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
