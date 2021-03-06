# ChatServer

##사용법
1. 반드시 release 브랜치의 파일을 다운받아주세요.
2. 폴더 안에 Release.zip파일이 있습니다.
3. Release.zip파일을 압축을 풀고 해당 폴더에서 콘솔창을 열어 
4. ChatServer listeningPort backEndIp backEndPort maxClientNum를 치면 시작 명령어 안내가 나옵니다.(maxClientNum: 한 서버에 접속가능한 최대 클라이언트 수)
5. 안전한 종료(FIN전송 포함)을 위해서는 ESCAPE 키를 눌러주세요..

##사용 환경
.Net FrameWork 4.5.2 이상.  
Visual Studio 2015.  
따로 사용한 라이브러리는 없습니다.

##기능
Client로부터 Request를 받아 BackEnd에 Request를 그대로 보내고, Response를 받으면 Response를 Client에도 보내줍니다.   
이때 SUCCESS를 받으면 그에 필요한 정보(Session, Room 등)를 저장합니다.  
  
채팅의 경우에는 BackEnd로 내용은 보내지 않고 서버가 같은 Room에 있는 User들에게 BroadCasting을 해주며, BackEnd에는 채팅이 왔다는   정보만 줌으로서 채팅 카운드를 올릴수 있게 해줍니다.  
  
Room에 있지 않은 경우 Room에 대한 요청을 하면 BackEnd로 보내지 않고 Fail을 바로 보내줍니다.  
Chat상태가 아닌경우 Chat에 대한 요청을 하면 BackEnd로 보내지 않고 Fail을 바로 보내줍니다.  
  
BackEnd가 접속이 끊기면 Back에 재접속함과 동시에 모든 방과 세션을 초기화 합니다.  
    
Client, BackEnd 모두로부터 FIN이 오는지 항상 검사하며, HealthCheck도 하고 있습니다.      
HealthCheck은 30초마다 보내며, 답이 오지 않은 경우 5초씩 기다려서 2번 더 보내보고 답이 오지 않으면 접속을 끊습니다.   

두개 이상의 서버를 띄우고 서로 다른 서버의 클라이언트가 같은방에 접속하려하면, 룸이 속해있는 서버의 IP와 Port를 BackEnd가 넘겨주며, 이를 Client에게 넘겨주면 Client가 해당 서버로 재접속합니다.
