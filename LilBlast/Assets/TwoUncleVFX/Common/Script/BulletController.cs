using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace VFXTools
{
	public enum TowardType
	{
		Forward,
		Right,
		Up
	}

	public class BulletController : MonoBehaviour
	{
		public float rotationSpeed = 100f;
		public float movementSpeed = 10f;
		public float delayTime = 0f;
		private bool isPlay = false;
		public float time = 1f;
		private float lastTime = 0f;
		private Vector3 startPos;
		public TowardType towardType = TowardType.Forward;
		private Vector3 directionToCenter;
		private Vector3 scale;
		private VisualEffect[] vfxs;
		private TrailRenderer[] trails;
		public float maxDistance = 100f;
		private float curDistance = 0f;

		private void OnEnable()
		{
			vfxs = GetComponentsInChildren<VisualEffect>(false);
			trails = GetComponentsInChildren<TrailRenderer>(false);
			startPos = transform.position;
			SetPlay(true);
		}

		private async void SetPlay(bool play)
		{
			// Eğer başlangıç gecikmesi gerekiyorsa burada ekleyebilirsin:
			// await Task.Delay((int)(delayTime * 1000));
			isPlay = play;
		}

		private void Update()
		{
			if (!isPlay) return;

			lastTime += Time.deltaTime;

			// Süre dolduysa efektleri durdur ve geri dönüşü başlat
			if (lastTime > time)
			{
				scale = transform.localScale;

				for (int i = 0; i < vfxs.Length; i++)
				{
					vfxs[i].enabled = false;
				}

				for (int i = 0; i < trails.Length; i++)
				{
					trails[i].enabled = false;
				}

				// Objeyi pasif hale getirmiyoruz — bunu ObjectPool yönetecek
				// Deaktif etmeden önce efektleri kapattık
				return;
			}

			// Geç başlama kontrolü veya max mesafe aşıldıysa işlem yapma
			if (delayTime > lastTime || curDistance > maxDistance)
			{
				return;
			}

			directionToCenter = transform.forward;
			Quaternion targetRotation = Quaternion.LookRotation(directionToCenter);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

			if (towardType == TowardType.Forward)
			{
				transform.Translate(Vector3.forward * movementSpeed * Time.deltaTime);
			}
			else if (towardType == TowardType.Right)
			{
				transform.Translate(Vector3.right * movementSpeed * Time.deltaTime);
			}
			else if (towardType == TowardType.Up)
			{
				transform.Translate(Vector3.up * movementSpeed * Time.deltaTime);
			}

			curDistance += movementSpeed * Time.deltaTime;
		}

		public async void DelayEnable()
		{
			await Task.Delay(500);

			// 🛡️ Obje pasifse hiçbir şeyi tekrar aktifleştirme
			if (!gameObject.activeInHierarchy)
				return;

			for (int i = 0; i < vfxs.Length; i++)
			{
				vfxs[i].enabled = true;
			}

			for (int i = 0; i < trails.Length; i++)
			{
				trails[i].enabled = true;
			}

			isPlay = true;
		}

		private void OnDisable()
		{
			//Debug.Log("disabled" + this.name);
			// Efekti sıfırla ama yeniden başlatma çağrısı yapma
			transform.localScale = Vector3.zero;
			transform.position = startPos;
			lastTime = 0f;
			curDistance = 0f;
			transform.localScale = scale;
			isPlay = false;

			// DelayEnable() burada çağrılmıyor artık — güvenli!
		}
	}
}
