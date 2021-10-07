using System.Collections;
using System.Collections.Generic;

namespace Mahjong.Command
{
    public interface ICommand
    {
        void Do(Context context);
    }
    /// <summary>
    /// ゲームの初期化
    /// </summary>
    public class Command_Initialize : ICommand
    {
        public void Do(Context context)
        {
            context.Initialize();
        }
    }
    /// <summary>
    /// 親決め
    /// </summary>
    public class Commnand_Oyakime : ICommand
    {
        public void Do(Context context)
        {

        }
    }
    /// <summary>
    /// 表ドラ開示
    /// </summary>
    public class Command_OpenDora : ICommand
    {
        public void Do(Context context)
        {
            //TODO
            for (int i = 0; i < 14; ++i)
            {
                var pai = context.GetPaiManager().Distribute();
                if (i == 5)
                {
                    //pai.SetDora
                }
            }
        }
    }
    /// <summary>
    /// 配牌
    /// </summary>
    public class Command_DistributePais : ICommand
    {
        public void Do(Context context)
        {
            foreach (var p in context.GetPlayers())
            {
                for (int i = 0; i < 13; ++i)
                {
                    var pai = context.GetPaiManager().Distribute();
                    p.Tsumo(pai);
                }
            }
        }
    }
    /// <summary>
    /// ターン開始
    /// </summary>
    public class Command_StartTurn : ICommand
    {
        public void Do(Context context)
        {
            //TODO
        }
    }
    /// <summary>
    /// ツモ
    /// </summary>
    public class Command_Tsumo : ICommand
    {
        public void Do(Context context)
        {
            var pai = context.GetPaiManager().Distribute();

            context.GetCurrentTurnPlayer().Tsumo(pai);
        }
    }
    /// <summary>
    /// 捨てる
    /// </summary>
    public class Command_Trash : ICommand
    {
        public void Do(Context context)
        {
            //TODO
            context.GetCurrentTurnPlayer().Trash(index: 13);
        }
    }
    /// <summary>
    /// 自分のターン終わり
    /// </summary>
    public class Command_EndTurn : ICommand
    {
        public void Do(Context context)
        {
            context.NextTurn();
        }
    }
    /// <summary>
    /// １局始まり
    /// </summary>
    public class Command_StartKyoku : ICommand
    {
        public void Do(Context context)
        {
            var paiMgr = context.GetPaiManager();
            paiMgr.Initialize((int)System.DateTime.UtcNow.Ticks);

            foreach(var p in context.GetPlayers())
            {
                p.Initialize(p.Index, p.Score);
            }
        }
    }
    /// <summary>
    /// １局終わり
    /// </summary>
    public class Command_EndKyoku : ICommand
    {
        public void Do(Context context)
        {
            //TODO
            var retry = false;
            context.NextKyoku(retry);
        }
    }
    /// <summary>
    /// 精算
    /// </summary>
    public class Command_Seisan : ICommand
    {
        public void Do(Context context)
        {

        }
    }
    /// <summary>
    /// リーチする
    /// ツモアガリする
    /// ロンアガリする
    /// ポンする
    /// チーする
    /// アンカンする
    /// ミンカンする
    /// リンシャン牌引く
    /// カンドラめくる
    /// チャンカンホウする
    /// 流局する
    /// </summary>
    public class Command_XXX : ICommand
    {
        public void Do(Context context)
        {

        }
    }

}
