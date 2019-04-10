namespace Supercell.Magic.Titan.Math
{
    using Supercell.Magic.Titan.DataStream;

    public class LogicVector2
    {
        public int m_x;
        public int m_y;

        public LogicVector2()
        {
        }

        public LogicVector2(int x, int y)
        {
            this.m_x = x;
            this.m_y = y;
        }

        public void Destruct()
        {
            this.m_x = 0;
            this.m_y = 0;
        }
        
        public void Add(LogicVector2 vector2)
        {
            this.m_x += vector2.m_x;
            this.m_y += vector2.m_y;
        }

        public LogicVector2 Clone()
        {
            return new LogicVector2(this.m_x, this.m_y);
        }

        public int Dot(LogicVector2 vector2)
        {
            return this.m_x * vector2.m_x + this.m_y * vector2.m_y;
        }

        public int GetAngle()
        {
            return LogicMath.GetAngle(this.m_x, this.m_y);
        }

        public int GetAngleBetween(int x, int y)
        {
            return LogicMath.GetAngleBetween(LogicMath.GetAngle(this.m_x, this.m_y), LogicMath.GetAngle(x, y));
        }

        public int GetDistance(LogicVector2 vector2)
        {
            int x = this.m_x - vector2.m_x;
            int distance = 0x7FFFFFFF;

            if ((uint) (x + 46340) <= 92680)
            {
                int y = this.m_y - vector2.m_y;

                if ((uint) (y + 46340) <= 92680)
                {
                    int distanceX = x * x;
                    int distanceY = y * y;

                    if ((uint) distanceY < (distanceX ^ 0x7FFFFFFFu))
                    {
                        distance = distanceX + distanceY;
                    }
                }
            }

            return LogicMath.Sqrt(distance);
        }

        public int GetDistanceSquared(LogicVector2 vector2)
        {
            int x = this.m_x - vector2.m_x;
            int distance = 0x7FFFFFFF;

            if ((uint) (x + 46340) <= 92680)
            {
                int y = this.m_y - vector2.m_y;

                if ((uint) (y + 46340) <= 92680)
                {
                    int distanceX = x * x;
                    int distanceY = y * y;

                    if ((uint) distanceY < (distanceX ^ 0x7FFFFFFFu))
                    {
                        distance = distanceX + distanceY;
                    }
                }
            }

            return distance;
        }

        public int GetDistanceSquaredTo(int x, int y)
        {
            int distance = 0x7FFFFFFF;

            x -= this.m_x;

            if ((uint) (x + 46340) <= 92680)
            {
                y -= this.m_y;

                if ((uint) (y + 46340) <= 92680)
                {
                    int distanceX = x * x;
                    int distanceY = y * y;

                    if ((uint) distanceY < (distanceX ^ 0x7FFFFFFFu))
                    {
                        distance = distanceX + distanceY;
                    }
                }
            }

            return distance;
        }

        public int GetLength()
        {
            int length = 0x7FFFFFFF;

            if ((uint) (46340 - this.m_x) <= 92680)
            {
                if ((uint) (46340 - this.m_y) <= 92680)
                {
                    int lengthX = this.m_x * this.m_x;
                    int lengthY = this.m_y * this.m_y;

                    if ((uint) lengthY < (lengthX ^ 0x7FFFFFFFu))
                    {
                        length = lengthX + lengthY;
                    }
                }
            }

            return LogicMath.Sqrt(length);
        }

        public int GetLengthSquared()
        {
            int length = 0x7FFFFFFF;

            if ((uint) (46340 - this.m_x) <= 92680)
            {
                if ((uint) (46340 - this.m_y) <= 92680)
                {
                    int lengthX = this.m_x * this.m_x;
                    int lengthY = this.m_y * this.m_y;

                    if ((uint) lengthY < (lengthX ^ 0x7FFFFFFFu))
                    {
                        length = lengthX + lengthY;
                    }
                }
            }

            return length;
        }

        public bool IsEqual(LogicVector2 vector2)
        {
            return this.m_x == vector2.m_x && this.m_y == vector2.m_y;
        }

        public bool IsInArea(int minX, int minY, int maxX, int maxY)
        {
            if (this.m_x >= minX && this.m_y >= minY)
                return this.m_x < minX + maxX && this.m_y < maxY + minY;
            return false;
        }

        public void Multiply(LogicVector2 vector2)
        {
            this.m_x *= vector2.m_x;
            this.m_y *= vector2.m_y;
        }

        public int Normalize(int value)
        {
            int length = this.GetLength();

            if (length != 0)
            {
                this.m_x = this.m_x * value / length;
                this.m_y = this.m_y * value / length;
            }

            return length;
        }

        public void Rotate(int degrees)
        {
            int newX = LogicMath.GetRotatedX(this.m_x, this.m_y, degrees);
            int newY = LogicMath.GetRotatedY(this.m_x, this.m_y, degrees);

            this.m_x = newX;
            this.m_y = newY;
        }

        public void Set(int x, int y)
        {
            this.m_x = x;
            this.m_y = y;
        }

        public void Substract(LogicVector2 vector2)
        {
            this.m_x -= vector2.m_x;
            this.m_y -= vector2.m_y;
        }

        public void Decode(ByteStream stream)
        {
            this.m_x = stream.ReadInt();
            this.m_y = stream.ReadInt();
        }

        public void Encode(ChecksumEncoder stream)
        {
            stream.WriteInt(this.m_x);
            stream.WriteInt(this.m_y);
        }

        public override string ToString()
        {
            return "LogicVector2(" + this.m_x + "," + this.m_y + ")";
        }
    }
}