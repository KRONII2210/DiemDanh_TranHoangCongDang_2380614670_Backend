namespace DiemDanh_TranHoangCongDang_2380614670_API.Services
{
    public static class QrWebPage
    {
        public static string GetHtml()
        {
            return @"<!DOCTYPE html>
<html lang=""vi"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Màn Hình Chiếu Giảng Viên - Mã QR Điểm Danh Động</title>
    <link href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700;800&display=swap"" rel=""stylesheet"">
    <style>
        * { box-sizing: border-box; margin: 0; padding: 0; font-family: 'Inter', sans-serif; }
        body {
            background: linear-gradient(135deg, #0f172a 0%, #1e1b4b 50%, #0f172a 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            color: #f8fafc;
            padding: 20px;
        }
        .container {
            background: rgba(30, 41, 59, 0.85);
            backdrop-filter: blur(16px);
            border: 1px solid rgba(255, 255, 255, 0.1);
            border-radius: 24px;
            padding: 36px 44px;
            max-width: 580px;
            width: 100%;
            text-align: center;
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
        }
        .badge {
            display: inline-block;
            background: rgba(59, 130, 246, 0.2);
            color: #60a5fa;
            border: 1px solid rgba(59, 130, 246, 0.4);
            padding: 6px 16px;
            border-radius: 999px;
            font-size: 13px;
            font-weight: 700;
            letter-spacing: 0.5px;
            margin-bottom: 14px;
        }
        h1 {
            font-size: 24px;
            font-weight: 800;
            color: #ffffff;
            margin-bottom: 8px;
        }
        .subtitle {
            font-size: 14px;
            color: #94a3b8;
            margin-bottom: 24px;
        }
        .qr-box {
            background: #ffffff;
            border-radius: 20px;
            padding: 24px;
            display: inline-block;
            box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.4);
            margin-bottom: 20px;
        }
        #qr-canvas {
            display: block;
            margin: 0 auto;
        }
        .token-display {
            background: #0f172a;
            border: 1px solid rgba(255, 255, 255, 0.15);
            border-radius: 12px;
            padding: 12px 20px;
            font-size: 20px;
            font-weight: 800;
            letter-spacing: 3px;
            color: #38bdf8;
            margin-bottom: 20px;
            font-family: monospace;
        }
        .timer-bar-container {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 999px;
            height: 10px;
            overflow: hidden;
            margin-bottom: 12px;
        }
        .timer-bar {
            height: 100%;
            width: 100%;
            background: linear-gradient(90deg, #3b82f6, #06b6d4);
            transition: width 1s linear;
        }
        .timer-text {
            font-size: 13px;
            color: #cbd5e1;
            font-weight: 600;
            margin-bottom: 20px;
        }
        .footer-note {
            font-size: 12px;
            color: #64748b;
            line-height: 1.6;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""badge"">MÀN HÌNH CHIẾU GIẢNG VIÊN</div>
        <h1>HỆ THỐNG ĐIỂM DANH QR CODE</h1>
        <div class=""subtitle"">Môn: LTDD01 - Lập trình di động | Buổi 1: Giới thiệu Flutter UI</div>

        <div class=""qr-box"">
            <div id=""qr-container"" style=""display: flex; justify-content: center; align-items: center; width: 240px; height: 240px;""></div>
        </div>

        <div class=""token-display"" id=""token-text"">TSV-1-K7X9A2</div>

        <div class=""timer-bar-container"">
            <div class=""timer-bar"" id=""timer-bar""></div>
        </div>
        <div class=""timer-text"">
            Mã đổi mới sau: <span id=""seconds-left"" style=""color: #38bdf8; font-weight: 800;"">30</span> giây
        </div>

        <div class=""footer-note"">
            Sinh viên dùng App di động quét mã QR trên màn hình chiếu để điểm danh.<br>
            Mã tự động đổi mỗi 30s để chống chụp hình gửi điểm danh hộ.
        </div>
    </div>

    <script src=""/qrcode.min.js""></script>
    <script>
        const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
        let seconds = 30;
        let currentToken = 'TSV-1-K7X9A2';

        async function syncTokenToServer(token) {
            try {
                await fetch('/api/Attendance/set-qr', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ token: token })
                });
            } catch (e) {
                console.log('Chua ket noi API hoac dang offline');
            }
        }

        function generateToken() {
            let code = '';
            for (let i = 0; i < 6; i++) {
                code += chars.charAt(Math.floor(Math.random() * chars.length));
            }
            currentToken = 'TSV-1-' + code;
            document.getElementById('token-text').textContent = currentToken;
            drawQR('DIEMDANH:1:' + currentToken);
            syncTokenToServer(currentToken);
        }

        let qrcodeObj = null;
        function drawQR(data) {
            const container = document.getElementById('qr-container');
            container.innerHTML = '';
            if (typeof QRCode !== 'undefined') {
                qrcodeObj = new QRCode(container, {
                    text: data,
                    width: 240,
                    height: 240,
                    colorDark: '#0f172a',
                    colorLight: '#ffffff',
                    correctLevel: QRCode.CorrectLevel.M
                });
            } else {
                container.innerHTML = '<div style=""color:black; font-weight:bold; font-size:13px; text-align:center;"">DIEMDANH:1:' + currentToken + '</div>';
            }
        }

        setInterval(() => {
            seconds--;
            if (seconds <= 0) {
                seconds = 30;
                generateToken();
            }
            document.getElementById('seconds-left').textContent = seconds;
            document.getElementById('timer-bar').style.width = ((seconds / 30) * 100) + '%';
        }, 1000);

        generateToken();
    </script>
</body>
</html>";
        }
    }
}
