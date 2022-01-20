using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class HeliController : MonoBehaviour
{
    //オーブがついているか
    public bool orbed = false;
    //直前、現在の座標、そこから計算する正規化された視線
    float old_x, old_y;
    float now_x, now_y;
    Vector2 sight = new Vector2(1, 0);
    //検知距離(laserならそのまま、shootならこの2倍)
    public float detectDist = 7.0f;
    //RedTriのprefab
    public GameObject redTri;
    //redTriの射出力、クールタイム、撃つ向き
    public float shootPower = 10.0f;
    bool coolShoot = false;
    Vector2 toPlayer = new Vector2(1,0);
    //警戒態勢の変数、StageManagerからtrueにされるとすぐ処理をしてfalseになる
    public bool cautionEnemy = false;
    //巡回path
    public bool isLinear = true;//falseならCatmullRom
    public float pathPeriod = 5.0f;//何秒で1周するか
    public int node = 4;//pathの頂点数
    public float[] x_arr = new float[10];
    public float[] y_arr = new float[10];
    Vector3[] path = new Vector3[10];
    public float graceCautionTime = 3.0f;//StageManagerによって更新される
    public bool inDamage = false;//ダメージ中にtrue
    public float invisibleTime = 2.0f;//ダメージ中無敵時間
    public int hp = 1;
    // Start is called before the first frame update
    void Start()
    {
        //現在位置を取得
        old_x = transform.position.x;
        old_y = transform.position.y;
        //pathを構成
        for(int i = 0; i < node; i++){
            path[i] = new Vector3(x_arr[i], y_arr[i], 0.0f);
        }
        Array.Resize<Vector3>(ref path, node);//必要な長さに
        //移動開始
        MoveLoop();
    }

    // Update is called once per frame
    void Update()
    {
        ScaleChange();
    }

    void FixedUpdate()
    {
        SightUpdate();

        if(orbed)
        {
            if(coolShoot == false)
            {
                SearchingShoot();
            }
        }
        else
        {
            SearchingLaser();
        }

        if(cautionEnemy) DoCaution();
    }

    void SightUpdate()
    {
        //現在位置を取得
        now_x = transform.position.x;
        now_y = transform.position.y;
        //移動していれば進行方向を更新
        if((now_x - old_x != 0.0f) || (now_y - old_y != 0.0f))
        {
            //direを更新
            float dx = now_x - old_x;
            float dy = now_y - old_y;
            float angle = Mathf.Atan2(dy, dx);
            sight = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            //old_x, old_yを更新
            old_x = now_x;
            old_y = now_y;
        }
        
    }

    void SearchingLaser()
    {
        Vector2 now_pos = new Vector2(now_x, now_y);
        RaycastHit2D hit = Physics2D.Raycast(now_pos, sight, detectDist, ~(1<<12));//(1<<12)はEnemy Layerを表す. NOT演算により, それ以外のレイヤーと衝突しうる設定になる.
        //Laser描画
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if(lineRenderer == null) return;//LineRendererが除去されていたら何もしない
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPosition(0, now_pos);
        if(hit)
        {
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            lineRenderer.SetPosition(1, now_pos + sight*detectDist);
        }
        //LaserがPlayerを検知したら報告
        if(hit)
        {
            if(hit.transform.gameObject.tag == "Player")
            {
                //caution通報
                GameObject canv = GameObject.FindGameObjectWithTag("Canvas");
                Stage1Manager man = canv.GetComponent<Stage1Manager>();
                man.cautionEnemy = true;
            }
        }
    }

    void SearchingShoot()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if(player != null)
        {
            if(Vector3.Distance(player.transform.position, transform.position) < 2.0f * detectDist)
            {
                //クールタイムをつける宣言
                coolShoot = true;
                //プレイヤーと同位置でなければ向けて撃つ、同位置なら右に撃つ
                float dx = player.transform.position.x - transform.position.x;
                float dy = player.transform.position.y - transform.position.y;
                if((dx != 0.0f) || (dy != 0.0f))
                {
                    float angle = Mathf.Atan2(dy, dx);
                    toPlayer = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
                else
                {
                    toPlayer = new Vector2(1,0);
                }
                GameObject redShoot = Instantiate(redTri, transform.position, Quaternion.identity);
                Rigidbody2D rbody_red = redShoot.GetComponent<Rigidbody2D>();
                rbody_red.AddForce(toPlayer * shootPower, ForceMode2D.Impulse);
                Invoke("BeCool", 2.0f);
                //SE
                SoundManager.soundManager.PlaySE(SEtype.EnemyAttack);
            }
        }
    }

    void BeCool()
    {
        coolShoot = false;
    }

    void ScaleChange()
    {
        float scale_y = transform.localScale.y;
        //画像の向き調整
        if(sight.x > 0)
        {
            transform.localScale = new Vector2( - scale_y, scale_y);//元々左向き画像のため、右を向かせたければ反転
        }
        else if (sight.x < 0)
        {
            transform.localScale = new Vector2( + scale_y, scale_y);
        }
    }

    void DoCaution()
    {
        cautionEnemy = false;
        //レーザー描画をやめる
        Destroy(GetComponent<LineRenderer>());
        //redorbfakeを取得、SetActive==trueにする
        GameObject redorbfake = transform.GetChild(0).gameObject;
        redorbfake.SetActive(true);
        //redorbfakeの点滅開始
        Blink blink = redorbfake.GetComponent<Blink>();
        blink.blinking = true;
        Invoke("DoWarning", graceCautionTime);
    }

    void DoWarning()
    {
        //redorbfake点滅終了
        GameObject redorbfake = transform.GetChild(0).gameObject;
        Blink blink = redorbfake.GetComponent<Blink>();
        blink.blinking = false;
        //orbedとなる
        orbed = true;
    }

    void MoveLoop()
    {
        if(isLinear)
        {
            transform.DOLocalPath(path, pathPeriod, PathType.Linear).SetEase(Ease.Linear);
        }
        else
        {
            transform.DOLocalPath(path, pathPeriod, PathType.CatmullRom).SetEase(Ease.Linear);
        }
        Invoke("MoveLoop", pathPeriod);
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
