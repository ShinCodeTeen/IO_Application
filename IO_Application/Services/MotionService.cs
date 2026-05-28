using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FASTECH;
using IO_Application.Interfaces;

namespace IO_Application.Services
{
    public class MotionService : IMotionService
    {
        public async Task ABSMove(int IBdId, int cmdPos, uint speed, CancellationToken token = default)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                int moveRs = EziMOTIONPlusELib.FAS_MoveSingleAxisAbsPos(IBdId, cmdPos, speed);

            
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task EnableServo(int IBdId,int status, CancellationToken token = default)
        {
            try
            {
                int Rs = EziMOTIONPlusELib.FAS_ServoEnable(IBdId, status);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task JogMove(int IBdId, uint speed, int direction, CancellationToken token = default)
        {
            try
            {
                int JogRs = EziMOTIONPlusELib.FAS_MoveVelocity(IBdId, speed, direction);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task LimitMove(int IBdId, uint speed, int direction, CancellationToken token = default)
        {
            try
            {
                int rs = EziMOTIONPlusELib.FAS_MoveToLimit(IBdId, speed, direction);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ResetAlarm(int IBdId,CancellationToken token = default)
        {
            try
            {
                int rs = EziMOTIONPlusELib.FAS_ServoAlarmReset(IBdId);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ServoMove(int IBdId, int cmdPos, uint speed, CancellationToken token = default)
        {
            try
            {
                int rs = EziMOTIONPlusELib.FAS_MoveSingleAxisIncPos(IBdId,cmdPos, speed);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
