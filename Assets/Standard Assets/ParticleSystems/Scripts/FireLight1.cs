using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Effects
{
    public class FireLight1 : MonoBehaviour
    {
        private float m_Rnd;
        private bool m_Burning = true;
        private Light m_Light;
        private Vector3 initialPosition; // Boshlang‘ich joylashuv

        [Header("Light Intensity Settings")]
        public float minIntensity = 1.5f; // Minimal yorug‘lik intensivligi
        public float maxIntensity = 2f; // Maksimal yorug‘lik intensivligi
        public float flickerSpeed = 0.5f; // Yorug‘likning o‘zgarish tezligi

        [Header("Light Movement Settings")]
        public float movementAmplitude = 0.05f; // Harakat amplitudasi (kichik qimirlar)
        public float movementSpeed = 0.2f; // Harakat tezligi (tabiiy qimirlar)

        private void Start()
        {
            m_Rnd = Random.value * 100; // Tasodifiy boshlang‘ich qiymat
            m_Light = GetComponent<Light>();
            initialPosition = transform.localPosition; // Boshlang‘ich joylashuvni saqlash
        }

        private void Update()
        {
            if (m_Burning)
            {
                // Yorug‘lik intensivligini tabiiy o‘zgarishi
                m_Light.intensity = Mathf.Lerp(
                    minIntensity,
                    maxIntensity,
                    Mathf.PerlinNoise(Time.time * flickerSpeed, m_Rnd)
                );

                // Harakatni tabiiyroq qilish (boshlang‘ich joy atrofida)
                float x = Mathf.PerlinNoise(m_Rnd + 0 + Time.time * movementSpeed, m_Rnd + 1 + Time.time * movementSpeed) - 0.5f;
                float y = Mathf.PerlinNoise(m_Rnd + 2 + Time.time * movementSpeed, m_Rnd + 3 + Time.time * movementSpeed) - 0.5f;
                float z = Mathf.PerlinNoise(m_Rnd + 4 + Time.time * movementSpeed, m_Rnd + 5 + Time.time * movementSpeed) - 0.5f;
                transform.localPosition = initialPosition + new Vector3(x, y, z) * movementAmplitude;
            }
        }

        public void Extinguish()
        {
            m_Burning = false;
            m_Light.enabled = false;
        }
    }
}
