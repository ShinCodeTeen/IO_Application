using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IO_Application.Interfaces
{
    public interface IMotionService
    {
        public Task ServoMove(int IBdId, int cmdPos, uint speed, CancellationToken token = default);
        public Task ABSMove(int IBdId, int cmdPos,uint speed, CancellationToken token = default);
        public Task JogMove(int IBdId,uint speed,int direction, CancellationToken token = default);

        public Task LimitMove(int IBdId, uint speed,int direction, CancellationToken token = default);
        public Task ResetAlarm(int IBdId,CancellationToken token = default);
        public Task EnableServo(int IBdId,int status,CancellationToken token = default);

    }
}
