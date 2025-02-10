using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 摄像机移动速度
    public float moveSpeed = 10f;
    // 鼠标灵敏度
    public float lookSensitivity = 2f;
    // 控制垂直视角的上下限（防止翻转）
    public float minY = -80f;
    public float maxY = 80f;

    // 内部变量：记录当前的旋转角度
    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        // 隐藏并锁定鼠标指针，便于第一人称视角控制
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // === 1. 鼠标控制视角旋转 ===

        // 获取鼠标移动的增量
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        // 限制 pitch 值，防止视角过高或过低
        pitch = Mathf.Clamp(pitch, minY, maxY);

        // 设定摄像机的新旋转
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // === 2. 键盘控制摄像机移动 ===

        // 使用 WASD 键来控制前后左右移动
        float horizontal = Input.GetAxis("Horizontal"); // A 和 D 键
        float vertical = Input.GetAxis("Vertical");     // W 和 S 键

        // 根据摄像机自身坐标计算方向移动
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
