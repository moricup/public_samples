#include<bits/stdc++.h>
using namespace std;

//typedef
typedef unsigned int UINT;
typedef unsigned long long ULL;
typedef long long LL;
typedef long double LD;
typedef pair<LL, LL> PLL;
typedef tuple<LL, LL, LL> TLL3;
typedef tuple<LL, LL, LL, LL> TLL4;
typedef set<LL, greater<LL> > setdownLL;
#define PQ priority_queue
typedef PQ<LL, vector<LL>, greater<LL> > pqupLL;
//container utill
#define ALL(v) (v).begin(),(v).end()
#define CR [](auto element1, auto element2){return element1>element2;}
#define LB lower_bound
#define UB upper_bound
#define PB push_back
#define MP make_pair
#define MT make_tuple
//constant
#define PI 3.141592653589793
//command
#define DP(H,W) vector<vector<LL> > dp((H),vector<LL>((W),0))
#define COUTD(a) cout << fixed << setprecision(10) << (a) << endl

template<class Abel> struct RSQ_BIT{//[1,n]
    vector<Abel> dat;
    long long n;
    Abel unity;

    RSQ_BIT(long long n_origin, Abel SUM_UNITY):dat(n_origin +1,SUM_UNITY), n(n_origin), unity(SUM_UNITY){}

    Abel sum_from_first(long long i){
        Abel s=0;
        while(i>0){
            s+=dat[i];
            i-= (i&(-i));
        }
        return s;
    }

    Abel sum(long long i, long long j){//[i,j)
        if(i>=j) return unity;
        Abel vl = sum_from_first(i-1);
        Abel vr = sum_from_first(j-1);
        return vr-vl;
    }

    void add(long long i, Abel x){
        while(i <= n){
            dat[i] += x;
            i += (i&(-i));
        }
    }
};

vector<LL> XY[200010];
RSQ_BIT<LL> BIT(200010,0);

int main(){
    LL H,W, M;
    cin >> H >> W >> M;
    LL X,Y;
    LL i,j;
    LL Xfi=H+1;
    for(i=1; i<=M; i++){
        cin >> X >> Y;
        XY[X].PB(Y);
        if(Y==1){
            Xfi=min(Xfi,X);
        }
    }
    for(i=1; i<=H; i++){
        XY[i].PB(W+1);
        sort(ALL(XY[i]));
    }
    LL ans=XY[1][0]-1;
    for(j=1; j<XY[1][0]; j++){
        BIT.add(j,1);
    }
    for(i=2; i<=H; i++){
        for(j=0; j<XY[i].size(); j++){
            if(XY[i][j]>W) break;
            if(BIT.sum(XY[i][j],XY[i][j]+1)==1){
                BIT.add(XY[i][j],-1);
            }
        }
        if(i>=Xfi){
            ans+=BIT.sum(1,W+1);
        }else{
            ans+=(XY[i][0]-1);
            ans+=BIT.sum(XY[i][0],W+1);
        }
    }
    cout << ans << endl;
    system("pause");
    return 0;
}