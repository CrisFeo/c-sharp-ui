winpath() {
  path="$(readlink -f "$1")"
  [ "${path:0:7}" == "/mnt/c/" ]
  path="${path:7}"
  path="$(tr -s '/' '\\' <<< "${path}")"
  printf 'C:\\%s\n' "${path}"
}

sys32='/mnt/c/Windows/System32'

powershell() {
  "$sys32/WindowsPowerShell/v1.0/powershell.exe" "$@"
}

tasklist() {
  "$sys32/tasklist.exe" "$@"
}

taskkill() {
  "$sys32/taskkill.exe" "$@"
}

psrun() {
  args="-WindowStyle Minimized -PassThru -FilePath \"$1\""
  if [ $# -gt 1 ]; then
    args="$args -ArgumentList \"${@:2}\""
  fi
  powershell -Command "(Start-Process $args).Id" | tr -d '\r'
}

pidup() {
  tasklist '/NH' '/FI' "PID eq $1" | grep 'Console'
}

user="$( \
  wslpath "$( \
    powershell -NoProfile -NonInteractive -Command "\$Env:UserProfile" \
  )" | \
  sed 's#\r##g' \
)"

dotnet_tools="$user/.dotnet/tools"
