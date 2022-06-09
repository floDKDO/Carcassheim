using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] PlateauRepre board;
    Camera mainCamera;
    [SerializeField] private float limitRay = 0.1f;
    [SerializeField] private float moveSpeed = 5.5f;

    [SerializeField] private float moveTreshold = 0.001f;

    private int last_scroll = 0;
    private int last_move = 0;
    [SerializeField] private float scrollSpeed = 0.7f;
    [SerializeField] private float Z_max = 0.46f;
    [SerializeField] private float Z_min = -1.6f;

    private Vector3 click_init_pos;
    private Vector3 lastMovePos;
    Vector3 dragOrigin;

    float original_dist;
    bool click_on = false, moving = false;

    [SerializeField] float nearPlane;
    Vector3 npos, origin_click_pos, origin_click_offset;

    [SerializeField] Text error_text;

    void Awake()
    {
        mainCamera = Camera.main;
        npos = transform.position;
        scrollSpeed *= 30;
        moveSpeed *= 30;
        limitRay *= 1.15f;
    }


    private void OnEnable()
    {
        board.OnBoardExpanded += limitUpdate;
    }


    private void OnDisable()
    {
        board.OnBoardExpanded -= limitUpdate;
    }

    private float linearCurve(int x, float a = 0, float b = 100)
    {
        if (a + x > b)
            return 1f;
        return (a + x) / b;
    }

    private float discriminateSign(float x)
    {
        return (x > 0 ? x * 1.4f : x);
    }

    private float clamp(float x)
    {
        return x < 1 ? (x > 0 ? x : 0) : 1;
    }

    private float amplitude(float z)
    {
        float val = (z - Z_min) / (Z_max - Z_min);
        return val * val;
    }

    public bool cameraUpdate()
    {
        bool ignore_touch = Input.touchCount <= 1;
        if (Input.GetMouseButtonDown(0) && ignore_touch)
        {
            click_on = true;
            click_init_pos = transform.position;
            lastMovePos = -Input.mousePosition + new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, nearPlane - transform.position.z);
            origin_click_pos = transform.position;
            origin_click_offset = mainCamera.ScreenToWorldPoint(lastMovePos) - origin_click_pos;
            return true;
        }
        else if (Input.GetMouseButtonUp(0) && click_on)
        {
            checkMove();
            click_on = false;
            bool lmoving = moving;
            moving = false;
            return lmoving;
        }
        else if (click_on)
        {
            if (!ignore_touch)
            {
                click_on = false;
                moving = false;
                return checkZoom();
            }
            checkMove();
            return true;
        }
        else if (!moving && (Input.mouseScrollDelta.y != 0 || Input.touchCount == 2))
        {
            return checkZoom();
        }
        last_scroll = 0;
        last_move = 0;
        return true;
    }

    bool checkZoom()
    {
        if (error_text != null)
            error_text.text = "MEN IT'S WORKING " + Input.touchCount;
        bool no_count = false;
        float delta = Input.mouseScrollDelta.y;
        if (Input.touchCount == 2)
        {
            Touch v0 = Input.GetTouch(0);
            Touch v1 = Input.GetTouch(1);
            no_count = true;
            if (last_scroll == 0)
            {
                original_dist = (v0.position - v1.position).magnitude;
                last_scroll++;
                if (error_text != null)
                    error_text.text = "DIOSTANCE " + original_dist;
                return true;
            }
            else
            {
                float dist = (v0.position - v1.position).magnitude;
                if (error_text != null)
                    error_text.text = "DIOSTANCE " + dist + " / " + original_dist;
                delta = original_dist / dist - 1;
            }
        }

        Vector3 nz = transform.position + new Vector3(0, 0, discriminateSign(delta) * Time.deltaTime * scrollSpeed * linearCurve(last_scroll, 100));
        last_scroll += 1;
        if (Z_min > nz.z)
            nz.z = Z_min;
        if (nz.z > Z_max)
            nz.z = Z_max;
        transform.position = nz;
        return no_count;
    }

    void checkMove()
    {
        Vector3 pos = -Input.mousePosition + new Vector3(mainCamera.pixelWidth, mainCamera.pixelHeight, nearPlane - transform.position.z);
        Vector3 vect = pos - lastMovePos;

        Vector3 w_pos = mainCamera.ScreenToWorldPoint(pos);
        Vector3 w_last_pos = mainCamera.ScreenToWorldPoint(lastMovePos);
        if (vect.sqrMagnitude > moveTreshold)
        {
            moving = true;
        }
        else
        {
            return;
        }
        last_move += 1;
        vect.z = 0;

        npos = w_pos + (origin_click_pos - transform.position) - origin_click_offset;


        float factor_z = 1f; // amplitude(transform.position.z) + 0.01f;
        npos.z = 0;
        if (npos.sqrMagnitude > limitRay * factor_z)
        {
            npos.Normalize();
            npos *= Mathf.Sqrt(limitRay * factor_z);
        }
        lastMovePos = pos;
    }

    void LateUpdate()
    {
        float z = transform.position.z;
        Vector3 pos = Vector3.Lerp(npos, transform.position, clamp(Time.deltaTime * moveSpeed));
        pos.z = z;
        transform.position = pos;
    }


    void limitUpdate()
    {
        limitRay = board.BoardRadius * 0.15f * 0.15f + 0.15f;
    }

}
