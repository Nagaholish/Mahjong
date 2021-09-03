using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    public class Context
    {
        public void Initialize()
        {
            Field = 0;
            Kyoku = 0;
            Honba = 0;
            TurnCount = 0;
            CurrentTurnPlayer = 0;
            CurrentOyaPlayer = 0;
            _players = new Player[4];
            for(int i=0; i< _players.Length; ++i)
            {
                var p = new Player();
                {
                    p.Initialize(i, 25000);
                }
                _players[i] = p;
            }
            _paiManager = new PaiManager();
            _paiManager.Initialize((int)System.DateTime.UtcNow.Ticks);  //TODO
        }

        /// <summary>
        /// ����=0
        /// ���=1
        /// ����=2
        /// �k��=3
        /// </summary>
        public int Field { get; private set; }
        /// <summary>
        /// ��
        /// �����=Field==0 && Kyoku==0
        /// ��I�[���X=Field==1 && Kyoku==3
        /// </summary>
        public int Kyoku { get; private set; }
        public int Honba { get; private set; }

        public int TurnCount { get; private set; }
        public int CurrentTurnPlayer { get; private set; }
        public int CurrentOyaPlayer { get; private set; }
        public int[] WinnerPlayers { get; private set; }
        public int WinnerNearestPlayer { get; private set; }

        public void NextTurn()
        {
            CurrentTurnPlayer += 1;
            CurrentTurnPlayer %= 4;
            ++TurnCount;
        }

        public void NextKyoku(bool retry)
        {
            if (retry)
            {
                Honba += 1;
                CurrentTurnPlayer = CurrentOyaPlayer;
            }
            else
            {
                Kyoku += 1;
                if (Kyoku > 3)
                {
                    Field += 1;
                    Kyoku = 0;
                }
                Honba = 0;
                CurrentOyaPlayer += 1;
                CurrentOyaPlayer %= 4;
                CurrentTurnPlayer = CurrentOyaPlayer;
            }
        }
        public bool IsEndKyoku()
        {
            //  ���� or �オ�����l������
            return GetPaiManager().IsEmpty();
        }
        public bool IsEndHanchan()
        {
            //  �I�[���X or �̓R��
            return Field == 1 && Kyoku == 3;
        }

        public int PlayerCount
        {
            get
            {
                if (_players == null) { return 0; }
                return _players.Length;
            }
        }

        public Player GetFocusPlayer()
        {
            return _players[CurrentTurnPlayer];
        }

        public IEnumerable<Player> GetPlayers()
        {
            return _players;
        }

        public PaiManager GetPaiManager()
        {
            return _paiManager;
        }

        private Player[] _players = null;
        private PaiManager _paiManager = null;
    }
}