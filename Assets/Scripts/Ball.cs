﻿using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {

    public float initialSpeed = 1f;
    public float maxSpeedAugmentationMult = 10f;
    public Transform rightPlayerTransform;
    public Transform leftplayerTransform;

    private ElementsNames element;
    private float speed;
    private bool goRight = true;
    private PlayerPosition currentPlayerTarget = PlayerPosition.RIGHT;
    private float distanceBetweenPlayers;
    private Vector3 moveDirection = Vector3.right;
    private Animator anim;

	// begin stuff
	private bool beginBool = true;
	public float timeToBegin;
	private float beginY;
	private float timer;
	private bool endOnce = false;
	public float speedFadeOutVolume;
	private AudioSource beginAudioClip;

    public  delegate void Winner(PlayerPosition player);
    public static event Winner winEvent;

    bool isKonamiMode = false;
    public KeySequencer konamiSequence;
    public Sprite KojimaHead;
    public SpriteRenderer ballSprite;
    public static bool gameBegin = false;


	//sound


	public AudioSource audio;

    void Start()
    {
        konamiSequence.SequenceValidEvent += Konami;
        anim = GetComponent<Animator>();
        speed = initialSpeed;
        distanceBetweenPlayers = Vector3.Distance(rightPlayerTransform.position, leftplayerTransform.position);
		beginY = transform.position.y;
		beginAudioClip = GetComponent<AudioSource> ();
    }

    void Konami()
    {
        isKonamiMode = true;
        ballSprite.sprite = KojimaHead;
        ballSprite.color = Color.white;
    }

    void Update()
    {
        if (gameBegin)
        {
            if (!beginBool)
            {
                Vector3 move = moveDirection * speed * Time.deltaTime;
                if (!goRight)
                {
                    move *= -1;
                }
                transform.position = transform.position + move;
                if (isKonamiMode)
                {
                    transform.Rotate(new Vector3(0, 0, 100) * speed * Time.deltaTime * ((goRight) ? -1 : 1));
                }
                switch (element)
                {
                    case ElementsNames.LIGHT:
                        anim.SetInteger("Num", 0);
                        break;
                    case ElementsNames.DARK:
                        anim.SetInteger("Num", 1);
                        break;
                    case ElementsNames.CHAOS:
                        anim.SetInteger("Num", 2);
                        break;
                    default:
                        break;
                }
                Debug.Log(element.ToString());
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= timeToBegin)
                {
                    if (!endOnce)
                    {
                        transform.position = new Vector3(transform.position.x, -2, transform.position.z);
                        SetType();
                        endOnce = true;
                    }
                    else
                    {
                        beginAudioClip.volume -= Time.deltaTime * speedFadeOutVolume;
                        if (beginAudioClip.volume == 0)
                        {
                            beginBool = false;
                        }
                    }
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Lerp(beginY, -2, timer / timeToBegin), transform.position.z);
                }
            }
        }
    }

    public bool GoRight
    {
        get { return goRight; }
        set { goRight = value; }
    }

    bool IsReverseType(ElementsNames type)
    {
        //switch (element)
        //{
        //    case ElementsNames.LIGHT:
        //        if(type == ElementsNames.CHAOS)
        //        {
        //            return true;
        //        }
        //        return false;
        //    case ElementsNames.DARK:
        //        if(type == ElementsNames.LIGHT)
        //        {
        //            return true;
        //        }
        //        return false;
        //    case ElementsNames.CHAOS:
        //        if(type == ElementsNames.DARK)
        //        {
        //            return true;
        //        }
        //        return false;
        //    default:
        //        return false;
        //}
        return (int)type == (int)element;
    }

    void SetType()
    {
        int rand = Random.Range(0, 3);
        element = (ElementsNames)rand;
        switch (element)
        {
            case ElementsNames.LIGHT:
                anim.SetInteger("Num", 0);
                break;
            case ElementsNames.DARK:
                anim.SetInteger("Num", 1);
                break;
            case ElementsNames.CHAOS:
                anim.SetInteger("Num", 2);
                break;
            default:
                break;
        }
        Debug.Log(element.ToString());
    }

    void ManageBallMovement()
    {
        Vector3 refPosition;
        switch (currentPlayerTarget)
        {
            case PlayerPosition.RIGHT:
                refPosition = rightPlayerTransform.position;
                goRight = false;
                currentPlayerTarget = PlayerPosition.LEFT;
                break;
            case PlayerPosition.LEFT:
                refPosition = leftplayerTransform.position;
                goRight = true;
                currentPlayerTarget = PlayerPosition.RIGHT;
                break;
            default:
                refPosition = rightPlayerTransform.position;
                break;
        }

        float distance = Vector3.Distance(refPosition, transform.position);
        speed = Mathf.Lerp(speed, speed * maxSpeedAugmentationMult, (distanceBetweenPlayers - distance) / distanceBetweenPlayers);

    }

    void OnTriggerEnter2D(Collider2D other)
    {
                    Debug.Log("Hit !");
        if(other.gameObject.tag == "Spell")
        {
            Spell sp = other.gameObject.GetComponent<Spell>();
            Debug.Log("Spell detected");
            if(sp.moveright != GoRight)
            {
                Debug.Log("Direction ok");
                if (IsReverseType(sp.type))
                {
                    Debug.Log("ChangeDirection");
                    audio.Play();
                    ManageBallMovement();
                    SetType();
                }
            }
            sp.AnimateHit();
        }
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("Player " + currentPlayerTarget.ToString() + " Win !");
            winEvent(currentPlayerTarget);
			/// do stuff
            Destroy(gameObject);
        }
    }
}
