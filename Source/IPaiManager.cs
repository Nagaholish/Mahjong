namespace Mahjong
{
    /// <summary>
    /// 牌マネージャ
    /// 山、残りの牌数、ドラを管理する
    /// </summary>
    public interface IPaiManager
    {
        /// <summary>
        /// 初期化する
        /// </summary>
        /// <param name="seed">乱数シード</param>
        /// <param name="shuffle">配牌用シャッフルをするか</param>
        /// <param name="wanpai">王牌を初期化して、王牌分の残数を減らすか</param>
        void Initialize(int seed, bool shuffle = true, bool wanpai = true);
        
        /// <summary>
        /// 王牌の初期化をする
        /// </summary>
        void InitializeWanpaiDoras();

        /// <summary>
        /// 牌を配る
        /// </summary>
        /// <returns></returns>
        DistributedPai Distribute();

        /// <summary>
        /// 山の牌が空になっているか？
        /// </summary>
        /// <returns></returns>
        bool IsEmpty();

        /// <summary>
        /// 山の牌の残り数を返す
        /// </summary>
        /// <returns></returns>
        int CountRemainedPais();
    }

}