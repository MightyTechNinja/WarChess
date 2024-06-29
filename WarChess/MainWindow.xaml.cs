using System.Diagnostics.Eventing.Reader;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WarChess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public enum Type
    {
        Empty,
        Knight,
        Shield,
        Archer,
        Lancer
    }

    public enum Team
    {
        Empty,
        B,
        R
    }

    public struct Soldier
    {
        public Image image;
        public Type type;
        public Team team;
        public int life;
        public int walk;
        public int x;
        public int y;
    }   

    public struct State
    {
        public int id;
        public Team team;
        public Type type;
        public int life;
    }

    public struct Castle
    {
        public Image image;
        public Team team;
    }

    public partial class MainWindow : Window
    {
        public State[,] board = new State[11, 11];
        public Soldier[] blue_sols = new Soldier[9];
        public Soldier[] red_sols = new Soldier[9];
        public Castle[] castles = new Castle[9];

        public Team turn;
        public Soldier selected_sol;
        public Image select = new Image();
        public bool is_selected;
        public int step;
        public Image[] real_moves = new Image[4];
        public Image[] temp_moves = new Image[4];

        public Team del_team;
        public int blue_castle_min;
        public int red_castle_min;

        public int[,] dirs = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
        public int[,] can_kill =  { { 1, 1, 1, 1, 1 },
                                    { 1, 1, 1, 1, 0 },
                                    { 1, 0, 1, 1, 0 },
                                    { 1, 1, 0, 1, 1 },
                                    { 1, 1, 1, 1, 1 } };

        public MainWindow()
        {
            InitializeComponent();
            InitBorder();
            InitCastle();
            InitSoldierAndBoard();
            InitSelect();
            InitDelete();

            turn = Team.R;
        }

        public void InitBorder()
        {
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    Border border = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                    BattleField.Children.Add(border);
                }
            }
        }

        public void InitCastle()
        {
            Team[] init_teams = { Team.Empty, Team.Empty, Team.R, Team.Empty, Team.Empty, Team.Empty, Team.B, Team.Empty, Team.Empty };
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int k = i * 3 + j;
                    castles[k].image = new Image();
                    castles[k].team = init_teams[k];
                    if (init_teams[k] == Team.Empty)
                    {
                        castles[k].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/Castle.png"));
                    }
                    else
                    {
                        castles[k].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/{init_teams[k]}Castle.png"));
                    }
                    Grid.SetColumn(castles[k].image, 5 * i);
                    Grid.SetRow(castles[k].image, 5 * j);
                    BattleField.Children.Add(castles[k].image);
                }
            }
        }

        public void InitSoldierAndBoard() { 
            Type[] init_types = { Type.Knight, Type.Knight, Type.Knight, Type.Shield, Type.Shield, Type.Archer, Type.Archer, Type.Lancer, Type.Lancer };
            int[,] init_poses = { { 0, 0 }, { 0, 1 }, { 0, 2 }, { 1, 0 }, { 1, 1 }, { 2, 0 }, { 2, 1 }, { 3, 0 }, { 3, 1 } };

            for (int i = 0; i < 9; i++)
            {
                blue_sols[i].image = new Image();
                blue_sols[i].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/B{init_types[i]}.png"));
                blue_sols[i].image.MouseDown += SoldierSelect;
                blue_sols[i].type = init_types[i];
                blue_sols[i].team = Team.B;
                blue_sols[i].life = 1;
                blue_sols[i].walk = blue_sols[i].type == Type.Knight ? 4 : 2;
                int col = 10 - init_poses[i, 0];
                int row = init_poses[i, 1];
                blue_sols[i].x = col;
                blue_sols[i].y = row;
                Grid.SetColumn(blue_sols[i].image, col);
                Grid.SetRow(blue_sols[i].image, row);
                BattleField.Children.Add(blue_sols[i].image);
                board[col, row].id = i;
                board[col, row].team = Team.B;
                board[col, row].type = init_types[i];
                board[col, row].life = 1;
            }

            for (int i = 0; i < 9; i++) {
                red_sols[i].image = new Image();
                red_sols[i].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/R{init_types[i]}.png"));
                red_sols[i].image.MouseDown += SoldierSelect;
                red_sols[i].type = init_types[i];
                red_sols[i].team = Team.R;
                red_sols[i].life = 1;
                red_sols[i].walk = red_sols[i].type == Type.Knight ? 4 : 2;
                int col = init_poses[i, 0];
                int row = 10 - init_poses[i, 1];
                red_sols[i].x = col;
                red_sols[i].y = row;
                Grid.SetColumn(red_sols[i].image, col);
                Grid.SetRow(red_sols[i].image, row);
                BattleField.Children.Add(red_sols[i].image);
                board[col, row].id = i;
                board[col, row].team = Team.R;
                board[col, row].type = init_types[i];
                board[col, row].life = 1;
            }            
        }

        public void InitSelect()
        {
            select.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Select.png"));
            select.Visibility = Visibility.Collapsed;
            BattleField.Children.Add(select);
        }

        public void InitDelete()
        {
            del_team = Team.Empty;
            blue_castle_min = 6;
            red_castle_min = 6;
        }

        public void ResetMove()
        {
            is_selected = false;
            select.Visibility = Visibility.Collapsed;
            Toolbar.Visibility = Visibility.Hidden;
            for (int i = 0; i < 4; i++)
            {
                BattleField.Children.Remove(real_moves[i]);
                BattleField.Children.Remove(temp_moves[i]);
            }
        } 
    
        public void SoldierSelect(object sender, MouseButtonEventArgs e)
        {
            if (!is_selected)
            {
                Image soldier = (Image)sender;
                int col = Grid.GetColumn(soldier);
                int row = Grid.GetRow(soldier);
                int id = board[col, row].id;
                Team team = board[col, row].team;
//                if (team != turn) return;
                is_selected = true;
                step = 0;
                selected_sol = team == Team.B ? blue_sols[id] : red_sols[id];
                if (IsSlow()) selected_sol.walk /= 2;
                Grid.SetColumn(select, col);
                Grid.SetRow(select, row);
                select.Visibility = Visibility.Visible;
                Toolbar.Visibility = Visibility.Visible;
                if (del_team == Team.Empty) ShowTempMove(col, row);
            }
        }

        public bool IsSlow()
        {
            if (castles[3].team == castles[4].team && castles[4].team == castles[5].team)
            {
                if (castles[4].team == Team.B && selected_sol.team == Team.R && selected_sol.x >= 5) return true;
                if (castles[4].team == Team.R && selected_sol.team == Team.B && selected_sol.x <= 5) return true;
            }
            if (castles[1].team == castles[4].team && castles[4].team == castles[7].team)
            {
                if (castles[4].team == Team.B && selected_sol.team == Team.R && selected_sol.y <= 5) return true;
                if (castles[4].team == Team.R && selected_sol.team == Team.B && selected_sol.y >= 5) return true;
            }
            return false;
        }

        public void ShowTempMove(int col, int row)
        {
            for (int i = 0; i < 4; i++)
            {
                int x = col + dirs[i, 0];
                int y = row + dirs[i, 1];
                if (x < 0 || x > 10) continue;
                if (y < 0 || y > 10) continue;
                if (x == selected_sol.x && y == selected_sol.y) continue;
                temp_moves[i] = new Image();
                if (board[x, y].team == 3 - selected_sol.team)
                {
                    temp_moves[i].Source = new BitmapImage(new Uri("pack://application:,,,/Image/Attack.png"));
                    if (selected_sol.type != Type.Archer && can_kill[(int)selected_sol.type, (int)board[x, y].type] > 0)
                    {
                        temp_moves[i].MouseDown += TempSelect;
                    }
                    if (selected_sol.type == Type.Shield && selected_sol.x % 5 == 0 && selected_sol.y % 5 == 0)
                    {
                        temp_moves[i].MouseDown += TempSelect;
                    }
                }
                else
                {
                    temp_moves[i].Source = new BitmapImage(new Uri("pack://application:,,,/Image/Move.png"));
                    temp_moves[i].MouseDown += TempSelect;
                }
                temp_moves[i].Opacity = 0.3;
                Grid.SetColumn(temp_moves[i], x);
                Grid.SetRow(temp_moves[i], y);
                BattleField.Children.Add(temp_moves[i]);
            }
        }

        public void ShowNearEnemy(int col, int row)
        {
            for (int i = 0; i < 4; i++)
            {
                int x = col + dirs[i, 0];
                int y = row + dirs[i, 1];
                if (x < 0 || x > 10) continue;
                if (y < 0 || y > 10) continue;
                temp_moves[i] = new Image();
                if (board[x, y].team == 3 - selected_sol.team)
                {
                    temp_moves[i].Source = new BitmapImage(new Uri("pack://application:,,,/Image/Attack.png"));
                }
                temp_moves[i].Opacity = 0.3;
                Grid.SetColumn(temp_moves[i], x);
                Grid.SetRow(temp_moves[i], y);
                BattleField.Children.Add(temp_moves[i]);
            }
        }

        public void TempSelect(object sender, MouseButtonEventArgs e)
        {
            Image move = (Image)sender;
            int col = Grid.GetColumn(move);
            int row = Grid.GetRow(move);
            real_moves[step] = new Image();
            if (board[col, row].team == 3 - selected_sol.team)
            {
                real_moves[step].Source = new BitmapImage(new Uri("pack://application:,,,/Image/Attack.png"));
            }
            else
            {
                real_moves[step].Source = new BitmapImage(new Uri("pack://application:,,,/Image/Move.png"));
            }
            Grid.SetColumn(real_moves[step], col);
            Grid.SetRow(real_moves[step], row);
            BattleField.Children.Add(real_moves[step]);
            for (int i = 0; i < 4; i++)
            {
                BattleField.Children.Remove(temp_moves[i]);
            }
            step++;
            if (step == selected_sol.walk)
            {
                if (selected_sol.type == Type.Archer) ShowNearEnemy(col, row);
                return;
            }
            ShowTempMove(col, row);
        }

        public void ApproveDown(object sender, MouseButtonEventArgs e)
        {
            if (del_team == selected_sol.team)
            {
                Kill(selected_sol.x, selected_sol.y);
                del_team = Team.Empty;
            }
            if (step > 0)
            {
                int col = Grid.GetColumn(real_moves[step - 1]);
                int row = Grid.GetRow(real_moves[step - 1]);
                if (board[col, row].team == selected_sol.team)
                {
                    ResetMove();
                    return;
                }
                if (col % 5 == 0 && row % 5 == 0
                    && board[col, row].team == 3 - selected_sol.team
                    && board[col, row].type == Type.Shield
                    && board[col, row].life == 2)
                {
                    if (step < 2)
                    {
                        ResetMove();
                        return;
                    }
                    col = Grid.GetColumn(real_moves[step - 2]);
                    row = Grid.GetRow(real_moves[step - 2]);
                    if (board[col, row].team == selected_sol.team)
                    {
                        ResetMove();
                        return;
                    }
                }
                KillEnemy();
                Move(col, row);
                ConquerCastle();
                turn = 3 - turn;
            }
            ResetMove();
        }

        public void Kill(int x, int y)
        {
            int id = board[x, y].id;
            if (board[x, y].team == Team.B)
            {
                blue_sols[id].life--;
                if (blue_sols[id].life == 0)
                {
                    BattleField.Children.Remove(blue_sols[id].image);
                    board[x, y].team = Team.Empty;
                }
                else
                {
                    blue_sols[id].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/BShield.png"));
                }
            }
            else
            {
                red_sols[id].life--;
                if (red_sols[id].life == 0)
                {
                    BattleField.Children.Remove(red_sols[id].image);
                    board[x, y].team = Team.Empty;
                }
                else
                {
                    red_sols[id].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/RShield.png"));
                }
            }
            board[x, y].life--;
        }

        public void Move(int col, int row)
        {
            board[col, row] = board[selected_sol.x, selected_sol.y];
            board[selected_sol.x, selected_sol.y].team = Team.Empty;
            int id = board[selected_sol.x, selected_sol.y].id;
            if (selected_sol.team == Team.B)
            {
                blue_sols[id].x = col;
                blue_sols[id].y = row;
            }
            else
            {
                red_sols[id].x = col;
                red_sols[id].y = row;
            }
            Grid.SetColumn(selected_sol.image, col);
            Grid.SetRow(selected_sol.image, row);
        }

        public void KillEnemy()
        {
            for (int i = 0; i < step; i++)
            {
                int x = Grid.GetColumn(real_moves[i]);
                int y = Grid.GetRow(real_moves[i]);
                if (board[x, y].team == 3 - selected_sol.team)
                {
                    Kill(x, y);
                }
            }
            if (selected_sol.type == Type.Archer)
            {
                int col = Grid.GetColumn(real_moves[step - 1]);
                int row = Grid.GetRow(real_moves[step - 1]);
                for (int i = 0; i < 4; i++)
                {
                    int x = col + dirs[i, 0];
                    int y = row + dirs[i, 1];
                    if (x < 0 || x > 10) continue;
                    if (y < 0 || y > 10) continue;
                    if (board[x, y].team == 3 - selected_sol.team && can_kill[(int)Type.Archer, (int)board[x, y].type] > 0)
                    {
                        Kill(x, y);
                    }
                }
            }
        }

        public void ConquerCastle()
        {
            for (int i = 0; i < step; i++)
            {
                int x = Grid.GetColumn(real_moves[i]);
                int y = Grid.GetRow(real_moves[i]);
                if (x % 5 == 0 && y % 5 == 0)
                {
                    int k = (x * 3 + y) / 5;
                    castles[k].team = board[x, y].team != Team.Empty ? board[x, y].team : selected_sol.team;
                    castles[k].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/{castles[k].team}Castle.png"));
                    if (i == step - 1 && selected_sol.type == Type.Shield)
                    {
                        int id = board[x, y].id;
                        if (selected_sol.team == Team.B)
                        {
                            blue_sols[id].life = 2;
                            blue_sols[id].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/BShield2.png"));
                        }
                        else
                        {
                            red_sols[id].life = 2;
                            red_sols[id].image.Source = new BitmapImage(new Uri($"pack://application:,,,/Image/RShield2.png"));
                        }
                        board[x, y].life = 2;
                    }
                    int blue_castle = 0, red_castle = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        if (castles[j].team == Team.B) blue_castle++;
                        if (castles[j].team == Team.R) red_castle++;
                    }
                    if (blue_castle == blue_castle_min)
                    {
                        MessageBox.Show("Red team must kill its own soldier.");
                        del_team = Team.R;
                        blue_castle_min++;
                    }
                    if (red_castle == red_castle_min)
                    {
                        MessageBox.Show("Blue team must kill its own soldier.");
                        del_team = Team.B;
                        red_castle_min++;
                    }
                }
            }
        }

        public void CancelDown(object sender, MouseButtonEventArgs e)
        {
            ResetMove();
        } 
    } 
} 