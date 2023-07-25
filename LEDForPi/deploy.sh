dotnet build
scp bin/Debug/net7.0/** pi3:/home/pi/test

ssh pi3 << EOF
tmux kill-session -t LED
tmux new-session -d -s "LED"\;

tmux send-keys -t "LED":0.0 "cd ~/test/" Enter
tmux send-keys -t "LED":0.0 "sudo dotnet LEDForPi.dll" Enter
tmux send-keys -t "LED":0.0 "12345678" Enter
EOF
