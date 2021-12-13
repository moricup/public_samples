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

long long extGCD(long long a, long long b, long long *px, long long *py){
    if(b == 0){
        *px = 1;
        *py = 0;
        return a;
    }
    long long d = extGCD(b, a%b, py, px);
    *py -= a/b * (*px);
    return d;
}

long long remainder(long long a, long long mod){
    if(a>=0){
        return a%mod;
    }else{
        return (a+(abs(a)/mod+1)*mod)%mod;
    }
}

int main(){
    LL T;
    LL N,S,K;
    cin >> T;
    LL g,x,y;
    LL NN;
    while(T>0, T--){
        cin >> N >> S >> K;
        K=K%N;
        g=extGCD(N,S,&x,&y);
        g=extGCD(g,K,&x,&y);
        N/=g;
        S/=g;
        K/=g;
        g=extGCD(N,K,&x,&y);
        if(S%g==0){
            y*=(S/g);
            NN=N/g;
            y=remainder(y,NN);
            y-=NN;
            cout << -y << endl;
        }else{
            cout << -1 << endl;
        }
    }
    system("pause");
    return 0;
}