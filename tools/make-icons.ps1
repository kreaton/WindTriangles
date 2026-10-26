param([string]$OutDir = (Join-Path $PSScriptRoot '..\src\WindTriangles\wwwroot\icons'))
# Generates the WindTriangles icon as SVG variants and renders the PNGs with headless Edge.
#   pwsh tools/make-icons.ps1
$ErrorActionPreference = 'Stop'
$inv = [Globalization.CultureInfo]::InvariantCulture
function F([double]$v) { $v.ToString('0.##', $inv) }

$work = Join-Path ([IO.Path]::GetTempPath()) 'windtriangles-icons'
New-Item -ItemType Directory -Force $work, $OutDir | Out-Null

function Arrow($x1, $y1, $x2, $y2, $color, $width, $startGap = 0, $endGap = 0) {
    $dx = $x2 - $x1; $dy = $y2 - $y1; $len = [Math]::Sqrt($dx * $dx + $dy * $dy)
    $ux = $dx / $len; $uy = $dy / $len
    # Keep heads and tails clear of the shared triangle corners
    $x1 += $ux * $startGap; $y1 += $uy * $startGap
    $x2 -= $ux * $endGap; $y2 -= $uy * $endGap
    $head = $width * 2.6
    $bx = $x2 - $ux * $head; $by = $y2 - $uy * $head
    $px = -$uy * $head * 0.62; $py = $ux * $head * 0.62
    "<line x1='$(F $x1)' y1='$(F $y1)' x2='$(F ($bx + $ux*2))' y2='$(F ($by + $uy*2))' stroke='$color' stroke-width='$width' stroke-linecap='round'/>" +
    "<polygon points='$(F $x2),$(F $y2) $(F ($bx+$px)),$(F ($by+$py)) $(F ($bx-$px)),$(F ($by-$py))' fill='$color' stroke='$color' stroke-width='$($width*0.35)' stroke-linejoin='round'/>"
}

# Artwork drawn on a 512 canvas centred at 256,256. $detail = false drops the fine ticks for tiny sizes.
function Artwork([bool]$detail) {
    $cx = 256; $cy = 256; $r = 176
    $s = "<circle cx='$cx' cy='$cy' r='$r' fill='none' stroke='#ffffff' stroke-opacity='0.92' stroke-width='$(if ($detail) {10} else {18})'/>"
    if ($detail) {
        for ($d = 0; $d -lt 360; $d += 10) {
            $len = if ($d % 90 -eq 0) { 34 } elseif ($d % 30 -eq 0) { 22 } else { 13 }
            $w = if ($d % 90 -eq 0) { 8 } else { 4 }
            $a = $d * [Math]::PI / 180
            $x1 = $cx + $r * [Math]::Sin($a); $y1 = $cy - $r * [Math]::Cos($a)
            $x2 = $cx + ($r - $len) * [Math]::Sin($a); $y2 = $cy - ($r - $len) * [Math]::Cos($a)
            $s += "<line x1='$(F $x1)' y1='$(F $y1)' x2='$(F $x2)' y2='$(F $y2)' stroke='#ffffff' stroke-opacity='0.85' stroke-width='$w' stroke-linecap='round'/>"
        }
    }
    # North pointer
    $s += "<polygon points='256,52 230,104 282,104' fill='#ff5a6e' stroke='#ff5a6e' stroke-width='6' stroke-linejoin='round'/>"
    # The current triangle: water (blue) + current (orange) = ground (green)
    $w = if ($detail) { 20 } else { 30 }
    $gap = if ($detail) { 16 } else { 10 }
    $s += Arrow 160 352 236 138 '#7cc4ff' $w 0 $gap
    $s += Arrow 236 138 364 214 '#ffad4d' $w $gap $gap
    $s += Arrow 160 352 364 214 '#5fe08a' $w 0 $gap
    $s += "<circle cx='160' cy='352' r='$(if ($detail) {15} else {22})' fill='#ffffff'/>"
    $s
}

$defs = "<defs><linearGradient id='bg' x1='0' y1='0' x2='0' y2='1'><stop offset='0' stop-color='#0d4d86'/><stop offset='1' stop-color='#062a4d'/></linearGradient></defs>"

# rounded: transparent corners (favicon, "any" icons). full: square bleed (Apple, which rounds itself).
# maskable: square bleed with the artwork inside the 80 % safe zone.
function Svg([string]$kind, [bool]$detail) {
    $bg = if ($kind -eq 'rounded') { "<rect width='512' height='512' rx='112' fill='url(#bg)'/>" } else { "<rect width='512' height='512' fill='url(#bg)'/>" }
    $scale = switch ($kind) { 'maskable' { 0.78 } 'full' { 0.9 } default { 1.0 } }
    $offset = 256 * (1 - $scale)
    "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 512 512'>$defs$bg<g transform='translate($(F $offset) $(F $offset)) scale($scale)'>$(Artwork $detail)</g></svg>"
}

$variants = @(
    @{ Name = 'icon.svg';            Kind = 'rounded';  Detail = $true;  Size = 0 },
    @{ Name = 'icon-1024.png';       Kind = 'rounded';  Detail = $true;  Size = 1024 },
    @{ Name = 'icon-512.png';        Kind = 'rounded';  Detail = $true;  Size = 512 },
    @{ Name = 'icon-192.png';        Kind = 'rounded';  Detail = $true;  Size = 192 },
    @{ Name = 'icon-512-maskable.png'; Kind = 'maskable'; Detail = $true; Size = 512 },
    @{ Name = 'apple-touch-icon.png'; Kind = 'full';    Detail = $true;  Size = 180 },
    @{ Name = 'apple-touch-icon-167.png'; Kind = 'full'; Detail = $true; Size = 167 },
    @{ Name = 'apple-touch-icon-152.png'; Kind = 'full'; Detail = $true; Size = 152 },
    @{ Name = 'favicon-32.png';      Kind = 'rounded';  Detail = $false; Size = 32 }
)

$edge = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
foreach ($v in $variants) {
    $svg = Svg $v.Kind $v.Detail
    if ($v.Size -eq 0) { Set-Content -Path (Join-Path $OutDir $v.Name) -Value $svg -Encoding utf8NoBOM; continue }
    $html = Join-Path $work ($v.Name + '.html')
    Set-Content -Path $html -Encoding utf8NoBOM -Value "<!doctype html><html><head><style>html,body{margin:0;background:transparent;overflow:hidden}svg{display:block;width:$($v.Size)px;height:$($v.Size)px}</style></head><body>$svg</body></html>"
    $png = Join-Path $OutDir $v.Name
    & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 --default-background-color=00000000 `
        --user-data-dir="$work\profile" --window-size="$($v.Size),$($v.Size)" --screenshot="$png" "file:///$($html -replace '\\','/')" 2>$null | Out-Null
    Start-Sleep -Milliseconds 300
    "{0,-28} {1}" -f $v.Name, (Test-Path $png)
}
