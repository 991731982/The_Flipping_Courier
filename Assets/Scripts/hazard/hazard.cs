using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Hazard : MonoBehaviour
{
    public GameObject[] lifeIcons;              // UI 小圖示
    public GameObject hitParticleEffect;        // 拖入你想播放的粒子效果 prefab

    private Dictionary<GameObject, int> playerHitCounts = new Dictionary<GameObject, int>();
    private const int maxHits = 3;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            GameObject playerObj = collision.gameObject;

            // ✅ 播放粒子效果（使用 GetComponentInChildren，支援子物件）
            if (hitParticleEffect != null)
            {
                UnityEngine.Debug.Log("✅ 播放特效開始");

                Vector3 spawnPos = playerObj.transform.position + Vector3.up * 1f;
                GameObject particle = Instantiate(hitParticleEffect, spawnPos, Quaternion.identity);

                ParticleSystem ps = particle.GetComponentInChildren<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play();
                    UnityEngine.Debug.Log("✅ 粒子播放成功");
                }
                else
                {
                    UnityEngine.Debug.Log("⚠ 沒有找到 ParticleSystem 組件！");
                }

                Destroy(particle, 3f);
            }
            else
            {
                UnityEngine.Debug.Log("❌ hitParticleEffect 沒有被指定！");
            }

            // 處理命數
            if (!playerHitCounts.ContainsKey(playerObj))
            {
                playerHitCounts[playerObj] = 0;
            }

            playerHitCounts[playerObj]++;
            int hits = playerHitCounts[playerObj];

            UnityEngine.Debug.Log($"Player hit hazard: {hits} time(s)");
            UpdateLifeUI(maxHits - hits);

            if (hits >= maxHits)
            {
                checkPointRespawn player = playerObj.GetComponent<checkPointRespawn>();
                if (player != null)
                {
                    UnityEngine.Debug.Log("Player reached max hits, respawning...");
                    player.RespawnAtCheckpoint();

                    playerHitCounts[playerObj] = 0;
                    UpdateLifeUI(maxHits); // 重設 UI 顯示
                }
            }
        }
    }

    private void UpdateLifeUI(int livesLeft)
    {
        UnityEngine.Debug.Log($"Updating life UI: lives left = {livesLeft}");

        int totalIcons = lifeIcons.Length;

        for (int i = 0; i < totalIcons; i++)
        {
            // Reverse the index so icons disappear from right to left
            int reversedIndex = totalIcons - 1 - i;
            lifeIcons[reversedIndex].SetActive(i < livesLeft);
        }
    }
}
