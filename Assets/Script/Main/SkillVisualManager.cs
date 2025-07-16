using UnityEngine;

public class SkillVisualManager : MonoBehaviour
{

    [SerializeField] private GameObject gunman;
    [SerializeField] private GameObject artillery;
    [SerializeField] private GameObject engineer;

    private Animator currentAnimator;

    public static SkillVisualManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetSkillVisual(SkillType skill)
    {
        // �X�L���^�C�v�Ō����ڂ�ς���
        gunman.SetActive(false);
        artillery.SetActive(false);
        engineer.SetActive(false);

        switch (skill)
        {
            case SkillType.Gunman:
                gunman.SetActive(true);
                currentAnimator = gunman.GetComponent<Animator>();
                break;
            case SkillType.Artillery:
                artillery.SetActive(true);
                currentAnimator = artillery.GetComponent<Animator>();
                break;
            case SkillType.Engineer:
                engineer.SetActive(true);
                currentAnimator = engineer.GetComponent<Animator>();
                break;
            default:
                currentAnimator = null;
                break;
        }
    }

    public void PlayShootAnimation(bool isArtillery, bool isSkillUse)
    {
        if (currentAnimator == null) return;

        // �X�L�����C���ŁA�X�L�����L���ȏꍇ�͕ʂ̃A�j���[�V����
        if (isArtillery && isSkillUse)
        {
            currentAnimator.Play("Shoot_Artillery", -1, 0f);
        }
        else
        {
            currentAnimator.Play("Shoot", -1, 0f); // �ʏ�̎ˌ��A�j���[�V����
        }
    }
}
