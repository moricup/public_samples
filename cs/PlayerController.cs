using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rbody;//自分のrbody
    //horizon方向
    float axisH = 0.0f;//horizon方向の向き
    public float walkspeed = 3.0f;//歩行速度
    public bool initDirectionIsRight = true;//初期の向き
    //vertical方向
    public float JumpPw_y = 9.0f;//ジャンプ力
    public LayerMask groundLayer;//地面のlayer
    bool goJump = false; //ジャンプボタンが押されたフラグ
    bool onGround = false;//地面にいるフラグ
    //Animation
    Animator animator;
    public string stopAnime = "PlayerStop";
    public string moveAnime = "PlayerMove";
    public string jumpAnime = "PlayerJump";
    public string deadAnime = "PlayerDead";
    public string attackAnime = "PlayerAttack";
    string nowAnime = "";
    string oldAnime = "";
    public bool orbed = false;
    public GameObject blueorb;
    GameObject bluewave;//Instantiateしたものを格納する
    public GameObject prefab_bluewave;
    public bool inDamage = false;//ダメージ中か
    public int orbedMaxHp = 12;
    public int notorbedMaxHp = 1;
    public int hp = 1;
    public float invisibleTime = 2.0f;//ダメージ後無敵時間
    public bool waveCoolNow = false;//trueの時はクールタイム中
    public float waveCoolTimeMax = 5.0f;//waveのクールタイムの時間
    float waveCoolTime = 0.0f;//クールタイムを記録
    public bool nowAttack = false;//攻撃中はtrueになる
    public GameObject hpText;//UI
    public GameObject wavePanel;//UI
    public GameObject wavePerText;//UI
    int wavePerInt;//小数点切り捨て用
    public ContactManager conman;
    public GameObject restartText;//restartを促すテキスト
    public bool isDead = false;//GameOver()でtrueになる

    // Start is called before the first frame update
    void Start()
    {
        //Player画像の向き調整
        if(initDirectionIsRight)
        {
            transform.localScale = new Vector2(1,1);
        }
        else
        {
            transform.localScale = new Vector2(-1,1);
        }
        //変数用意
        rbody = this.GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        nowAnime = stopAnime;
        oldAnime = stopAnime;
        //bluewave = transform.GetChild(1).gameObject;
        //hpの設定
        if(conman == ContactManager.Stage1_Boss)
        {
            hp = Stage1_BossManager.playerMaxHp;
        }
        else
        {
            if(orbed)
            {
                hp = orbedMaxHp;
            }
            else
            {
                hp = notorbedMaxHp;
            }
        }
        //UI更新
        RewriteHpText();
    }

    // Update is called once per frame
    void Update()
    {
        if(hp <= 0) return;//deadしたら何もしない

        axisH = Input.GetAxisRaw("Horizontal");
        //Player画像の向き調整
        ScaleChange();

        if(Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Z))
        {
            Jump();
        }

        //Playerのorbed, blueorbのactiveを同期
        ActiveOrb();

        //Xボタンで攻撃
        if(Input.GetKeyDown(KeyCode.X))
        {
            if((orbed == true) && (waveCoolNow == false)) AttackWave();
        }
    }

    void FixedUpdate()
    {
        if(hp <= 0) return;//deadしたら何もしない

        JumpMotion();//ジャンプの物理処理
        ChangeAnimation();
        //waveのクールタイム処理
        if(waveCoolNow) CoolingWave();
    }

    void ChangeAnimation()
    {
        //攻撃中なら何もしない
        if(nowAttack) return;
        //アニメーション切り替え
        if(onGround)
        {
            //地面
            if(axisH == 0)
            {
                //入力無し
                nowAnime = stopAnime;
            }
            else
            {
                //入力有り
                nowAnime = moveAnime;
            }
        }
        else
        {
            //空中
            nowAnime = jumpAnime;
        }
        //アニメーション切り替えありなら再生処理
        if(oldAnime != nowAnime)
        {
            oldAnime = nowAnime;
            animator.Play(nowAnime);
        }
    }

    void ScaleChange()
    {
        if(axisH > 0.0f)
        {
            transform.localScale = new Vector2(1,1);
        }
        else if(axisH < 0.0f)
        {
            transform.localScale = new Vector2(-1,1);
        }
    }

    void Jump()
    {
        goJump = true;
    }

    void JumpMotion()
    {
        //地上にいるか
        onGround = Physics2D.Linecast(transform.position, transform.position - (transform.up*0.1f), groundLayer);
        //空中&&入力無し(自由落下)でなければ速度を人工的に更新
        if(onGround || (axisH!=0))
        {
            rbody.velocity = new Vector2(walkspeed*axisH, rbody.velocity.y);    
        }
        //地面の上でジャンプボタンが押された時の挙動
        if(onGround && goJump)
        {
            goJump = false;
            Vector2 JumpPw = new Vector2(0, JumpPw_y);
            rbody.AddForce(JumpPw, ForceMode2D.Impulse);
            SoundManager.soundManager.PlaySE(SEtype.PlayerJump);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(hp <= 0) return;//deadしたら何もしない

        GameObject obj = collider.gameObject;
        //ダメージを受けるか
        if(inDamage == false)
        {
            if(obj.tag=="Bullet")
            {
                if(orbed)
                {
                    Rigidbody2D rbody_obj = obj.GetComponent<Rigidbody2D>();
                    rbody_obj.velocity = - rbody_obj.velocity;
                }
                else
                {
                    DamageStart(1);
                }
            }
            if(obj.tag=="Bomb")
            {
                DamageStart(1);
            }
        }
        //ItemOrbを取得か
        if(obj.tag=="ItemOrb")
        {
            GetOrb();
            Destroy(obj);//アイテムアイコンを消去
        }
        //Deathオブジェクトに触れたか
        if(obj.tag=="Death") DamageStart(orbedMaxHp);
    }

    void DamageStart(int damage_val)
    {
        inDamage = true;//ダメージ判定
        hp -= damage_val;//ダメージ計算
        hp = Mathf.Max(hp, 0);
        //hpが無くなったらGameOver
        if(hp <= 0)
        {
            GameOver();
            return;
        }
        //点滅
        Blink blink = GetComponent<Blink>();
        blink.blinking = true;
        //invisibleTime後ダメージ判定再開
        Invoke("DamageEnd", invisibleTime);
        //UI更新
        RewriteHpText();
        SoundManager.soundManager.PlaySE(SEtype.PlayerDamage);
    }

    void DamageEnd()
    {
        //ダメージ時間終了
        inDamage = false;
        //点滅終了
        Blink blink = GetComponent<Blink>();
        blink.blinking = false;
    }

    void GameOver()
    {
        //停止して死亡アニメーション
        rbody.velocity = Vector2.zero;
        rbody.gravityScale = 0.0f;
        animator.Play(deadAnime);
        SoundManager.soundManager.StopBGM();//BGM停止
        SoundManager.soundManager.PlaySE(SEtype.PlayerDead);
        restartText.SetActive(true);//restartを促すテキストを表示
        isDead = true;
    }

    void ActiveOrb()
    {
        if(orbed)
        {
            if(blueorb.activeSelf == false) blueorb.SetActive(true);
        }
        else
        {
            if(blueorb.activeSelf == true ) blueorb.SetActive(false);
        }
    }

    void AttackWave()
    {
        waveCoolNow = true;
        waveCoolTime = 0.0f;
        bluewave = Instantiate(prefab_bluewave, blueorb.transform.position, Quaternion.identity);
        //攻撃アニメーション
        nowAttack = true;
        animator.Play(attackAnime);
        Invoke("EndWave", 1.0f);//攻撃時間が1.0秒のため1.0秒としている
        //攻撃アニメーション解除のためのoldAnime設定
        oldAnime = attackAnime;
        //SE再生
        SoundManager.soundManager.PlaySE(SEtype.PlayerWave);
    }

    void CoolingWave()
    {
        waveCoolTime += Time.deltaTime;
        if((waveCoolTime/waveCoolTimeMax) < 1.0f)
        {
            wavePerInt = (int)((waveCoolTime/waveCoolTimeMax)*100.0f);
            wavePerText.GetComponent<Text>().text = wavePerInt.ToString();
        }
        else
        {
            EndCoolWave();
        }
    }

    void EndWave()
    {
        nowAttack = false;
    }

    void EndCoolWave()
    {
        waveCoolNow = false;
        wavePerText.GetComponent<Text>().text = (100).ToString();
    }

    void GetOrb()
    {
        orbed = true;
        hp = orbedMaxHp;
        //ItemOrb取得により通報される演出
        //warning通報
        GameObject canv = GameObject.FindGameObjectWithTag("Canvas");
        Stage1Manager stage_man = canv.GetComponent<Stage1Manager>();
        stage_man.warningEnemyJrs = true;
        //Signs更新
        GameObject signs = GameObject.FindGameObjectWithTag("Signs");
        SignsManager signs_man = signs.GetComponent<SignsManager>();
        signs_man.changeSigns = true;
        //UI更新
        RewriteHpText();
        wavePanel.SetActive(true);
        //本当はここでSE再生したいが、StageManagerのStopとの順序を保つため、StageManagerで再生
        //SoundManager.soundManager.PlaySE(SEtype.PlayerGetOrb);
    }

    void RewriteHpText()
    {
        if(hpText != null) hpText.GetComponent<Text>().text = hp.ToString();
    }
}
