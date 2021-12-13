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
    LL N;
    cin >> N;
    LL val;
    LL i;
    LL ans=0;
    bool nai;
    for(i=1; i<=N; i++){
        nai=1;
        val=i;
        while(val>0){
            if(val%8==7) nai=0;
            val/=8;
        }
        val=i;
        while(val>0){
            if(val%10==7) nai=0;
            val/=10;
        }
        if(nai==1) ans++;
    }
    cout << ans << endl;
    system("pause");
    return 0;
}