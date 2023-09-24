using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace EPI.Comm.Buffers
{
    /// <summary>
    /// 질문 : 큐버퍼와 리프레시 버퍼를 만든이유
    /// 패킷 수신 방식에 옵션을 주고자 함
    /// 큐방식은 -> 말그대로 완성될 때 까지 기다리는방식, 한번에 완성 안될 것을 대비하는 것
    /// 리프레시 방식1은 -> 한번에 패킷하나가 모두 수신될 것으로 예상하는 방식, 사이즈가 작거나 크면 무시함.
    /// 리프레시 방식2는 한번에 패킷이 지정된 사이즈보다 작거나, 큰사이즈가 오면 의도적으로 예외를 던져서 알려주는 방식
    /// 이거 고민중...인데 어쨋든 이 IBuffer는 적절한 인터페이스 설계인가?
    /// 
    /// </summary>
    internal interface IBuffer : IEnumerable<byte>
    {
        int Count { get; }
        byte[] GetBytes(int count);

        void AddBytes(byte[] bytes);
        void Clear();
    }
}
