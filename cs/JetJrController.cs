using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetJrController : MonoBehaviour
{
    Rigidbody2D rbody;//自分のrbody
    public bool activated = true;//待機中でなければtrue
    public float detectHoriDist = 20.0f;//プレイヤー検知距離
    public float closeHoriDist = 0.5f;//プレイヤーの真上位置の許容誤差
    public bool directionIsRight = true;//向き
    public float flySpeed = 9.0f;//推進無しの速度
    bool droppedBomb = false;//今の進行方向でBombを落としたか
    Vector2 oldVelocity;//停止前の速度
    public float stoppingTime = 0.5f;//Bombを落とす前後の停止時間
    public GameObject prefabFallingBomb;
    bool gone = false;//FirstGo実行時にtrueとなる
    public bool inDamage = false;//ダメージ中にtrue
    public float invisibleTime = 2.0f;//ダメージ中無敵時間
    public int hp = 1;
    // Start is called before the first frame update
    void Start()
    {
        //変数用意
        rbody = this.GetComponent<Rigidbody2D>();
        //画像の向き調整
        ScaleChange();
        //初めからactivatedであればここで加速
        if(activated == true)
        {
            Invoke("FirstGo", 0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //画像の向き調整
        ScaleChange();
    }

    void FixedUpdate()
    {
        //Playerを探す、いなければ速度を0として何もしない
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player == null)
        {
            rbody.velocity = new Vector2(0,0);
            return;
        }

        //自分、プレイヤーの座標を取得
        float jetJr_x = transform.position.x;
        float player_x = player.transform.position.x;

        //activatedでなければ、プレイヤーが近くにいるか確認
        if(activated == false)
        {
            if(Mathf.Abs(jetJr_x - player_x) > detectHoriDist)
            {
                //遠くにいるため何もしない
                return;
            }
            //近くに来たためactivateする、これ以降activated==trueのまま
            activated = true;
            //最初の加速
            FirstGo();
        }

        if(gone == false) return;//FirstGo前は以下に進まない

        //進行方向に応じて、Playerから離れたら方向転換し、falledBombをリセット
        //反転に少しラグをつける
        float rugTime = Random.Range(0.0f, 0.5f);
        if(directionIsRight)
        {
            if(jetJr_x - player_x > detectHoriDist)
            {
                directionIsRight = !directionIsRight;//逆を向く
                Invoke("ReverseAndGo", rugTime);
            }
        }
        else
        {
            if(player_x - jetJr_x > detectHoriDist)
            {
                directionIsRight = !directionIsRight;//逆を向く
                Invoke("ReverseAndGo", rugTime);
            }
        }

        //Bombを落としておらず、Playerの真上に来たらBombを落とす一連の動きをする
        if(droppedBomb == false)
        {
            if(Mathf.Abs(jetJr_x - player_x) < closeHoriDist)
            {
                droppedBomb = true;//Bombを落とすため、ここでtrueにしておく
                oldVelocity = rbody.velocity;//速度を記録
                rbody.velocity = new Vector2(0,0);//停止
                Invoke("DroppingBomb", stoppingTime);//少し停止した後DroppingBombに移行
            }
        }
    }

    void ScaleChange()
    {
        float scale_y = transform.localScale.y;
        //画像の向き調整
        if(directionIsRight)
        {
            transform.localScale = new Vector2( - scale_y, scale_y);//元々左向き画像のため、右を向かせたければ反転
        }
        else
        {
            transform.localScale = new Vector2( + scale_y, scale_y);
        }
    }

    //最初の加速
    void FirstGo()
    {
        if(directionIsRight)
        {
            rbody.velocity = new Vector2( + flySpeed, 0);//これから右へ向かう
        }
        else
        {
            rbody.velocity = new Vector2( - flySpeed, 0);//これから左へ向かう
        }
        gone = true;//出発した
    }

    //Playerから離れたら方向転換し、falledBombをリセット
    void ReverseAndGo()
    {
        if(droppedBomb)
        {
            rbody.velocity = -(float)0.5 * rbody.velocity ; //逆向きの2倍加速前の速度
        }
        else
        {
            rbody.velocity = - rbody.velocity;//逆向きの速度
        }
        droppedBomb = false;//リセット
    }
    void DroppingBomb()
    {
        //FallingBombを生成する
        Instantiate(prefabFallingBomb, transform.position, Quaternion.identity);
        //また少しした後Regoへ移行
        Invoke("Rego", stoppingTime);
        //SE再生
        SoundManager.soundManager.PlaySE(SEtype.FireMissile);
    }

    void Rego()
    {
        rbody.velocity = 2*oldVelocity;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject obj = collider.gameObject;
        //ダメージを受けるか
        if(inDamage == false)
        {
            if(obj.tag=="Wave")
            {
                DamageStart(1);
            }
        }
    }

    void DamageStart(int damage_val)
    {
        inDamage = true;//ダメージ判定
        hp -= damage_val;//ダメージ計算
        //点滅
        Blink blink = GetComponent<Blink>();
        blink.blinking = true;
        //invisibleTime後ダメージ判定再開
        Invoke("DamageEnd", invisibleTime);
        //SE
        SoundManager.soundManager.PlaySE(SEtype.EnemyDamage);
    }

    void DamageEnd()
    {
        //ダメージ時間終了
        inDamage = false;
        //点滅終了
        Blink blink = GetComponent<Blink>();
        blink.blinking = false;
        //hpが0なら消滅
        if(hp <= 0)
        {
            SoundManager.soundManager.PlaySE(SEtype.EnemyDestroy);
            Destroy(gameObject);
        }
    }
}
