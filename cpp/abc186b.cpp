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

int main(){
    LL H,W;
    cin >> H >> W;
    LL A[H][W];
    LL i,j;
    LL sai=10000;
    for(i=0; i<H; i++){
        for(j=0; j<W; j++){
            cin >> A[i][j];
            sai=min(sai,A[i][j]);
        }
    }
    LL ans=0;
    for(i=0; i<H; i++){
        for(j=0; j<W; j++){
            ans+=(A[i][j]-sai);
        }
    }
    cout << ans << endl;
    system("pause");
    return 0;
}