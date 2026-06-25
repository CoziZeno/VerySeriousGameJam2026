using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArenaCheck : MonoBehaviour
{
    [Header("Refs")]
    public Transform bridge;

    [Header("Arena")]
    public float emptyDelay = 1.5f;
    public Vector3 bridgeOpenEuler = new Vector3(0f, 0f, 0f);

    [Header("Bridge Motion")]
    public float bridgeOpenDuration = 1f;
    public AnimationCurve bridgeOpenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    readonly HashSet<SpinnerController> _enemiesInside = new HashSet<SpinnerController>();
    Coroutine _openBridgeRoutine;
    bool _bridgeOpened;

    void Start()
    {
        RefreshEnemiesInside();
        CheckArenaEmpty();
    }

    void OnTriggerEnter(Collider other)
    {
        SpinnerController spinner = other.GetComponentInParent<SpinnerController>();
        if (!IsValidEnemy(spinner))
            return;

        _enemiesInside.Add(spinner);

        if (_openBridgeRoutine != null)
        {
            StopCoroutine(_openBridgeRoutine);
            _openBridgeRoutine = null;
        }
    }

    void OnTriggerExit(Collider other)
    {
        SpinnerController spinner = other.GetComponentInParent<SpinnerController>();
        if (spinner != null)
            _enemiesInside.Remove(spinner);

        CheckArenaEmpty();
    }

    void Update()
    {
        if (_bridgeOpened)
            return;

        RemoveInvalidEnemies();
        CheckArenaEmpty();
    }

    void CheckArenaEmpty()
    {
        if (_bridgeOpened || _openBridgeRoutine != null)
            return;

        if (GetEnemyCountInside() == 0)
            _openBridgeRoutine = StartCoroutine(OpenBridgeAfterDelay());
    }

    IEnumerator OpenBridgeAfterDelay()
    {
        yield return new WaitForSeconds(emptyDelay);

        RemoveInvalidEnemies();

        if (GetEnemyCountInside() > 0)
        {
            _openBridgeRoutine = null;
            yield break;
        }

        if (bridge != null)
            yield return StartCoroutine(OpenBridgeSmoothly());

        _bridgeOpened = true;
        _openBridgeRoutine = null;
    }

    IEnumerator OpenBridgeSmoothly()
    {
        Quaternion startRotation = bridge.localRotation;
        Quaternion targetRotation = Quaternion.Euler(bridgeOpenEuler);

        if (bridgeOpenDuration <= 0f)
        {
            bridge.localRotation = targetRotation;
            yield break;
        }

        float timer = 0f;

        while (timer < bridgeOpenDuration)
        {
            timer += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(timer / bridgeOpenDuration);
            float curveValue = bridgeOpenCurve != null
                ? bridgeOpenCurve.Evaluate(normalizedTime)
                : normalizedTime;

            bridge.localRotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
            yield return null;
        }

        bridge.localRotation = targetRotation;
    }

    int GetEnemyCountInside()
    {
        RemoveInvalidEnemies();
        return _enemiesInside.Count;
    }

    void RefreshEnemiesInside()
    {
        _enemiesInside.Clear();

        Collider arenaCollider = GetComponent<Collider>();
        if (arenaCollider == null)
            return;

        Bounds bounds = arenaCollider.bounds;
        Collider[] hits = Physics.OverlapBox(bounds.center, bounds.extents, transform.rotation);

        for (int i = 0; i < hits.Length; i++)
        {
            SpinnerController spinner = hits[i].GetComponentInParent<SpinnerController>();
            if (IsValidEnemy(spinner))
                _enemiesInside.Add(spinner);
        }
    }

    void RemoveInvalidEnemies()
    {
        _enemiesInside.RemoveWhere(enemy => !IsValidEnemy(enemy));
    }

    bool IsValidEnemy(SpinnerController spinner)
    {
        return spinner != null &&
               spinner.isEnemy &&
               spinner.IsAlive &&
               spinner.gameObject.activeInHierarchy;
    }
}
