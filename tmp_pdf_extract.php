<?php
$path = 'C:/Users/user/Downloads/Telegram Desktop/12. PHP front controller, Router, View.pdf';
$raw = file_get_contents($path);

function inflateStream(string $s): string|false {
    foreach (['gzuncompress','gzinflate','gzdecode'] as $fn) {
        $d = @$fn($s);
        if ($d !== false) return $d;
    }
    return false;
}

function parseCmap(string $cmap): array {
    $map = [];
    if (preg_match_all('/<([0-9A-Fa-f]+)>\s*<([0-9A-Fa-f]+)>/', $cmap, $m, PREG_SET_ORDER)) {
        foreach ($m as $row) {
            if (strlen($row[1]) > 4) continue; // skip codespacerange-ish long
            $src = hexdec($row[1]);
            $dst = $row[2];
            $chars = '';
            for ($i = 0; $i < strlen($dst); $i += 4) {
                $chars .= mb_chr(hexdec(substr($dst, $i, 4)), 'UTF-8');
            }
            $map[$src] = $chars;
        }
    }
    return $map;
}

preg_match_all('/(\d+)\s+0\s+obj\s*(.*?)endobj/s', $raw, $objs, PREG_SET_ORDER);
$byId = [];
foreach ($objs as $o) $byId[(int)$o[1]] = $o[2];

function objStream(array $byId, int $id): string {
    $body = $byId[$id] ?? '';
    if (!preg_match('/stream\r?\n(.*?)\r?\nendstream/s', $body, $sm)) return '';
    return inflateStream($sm[1]) ?: '';
}

$fonts = [
    'F1' => parseCmap(objStream($byId, 524)),
    'F2' => parseCmap(objStream($byId, 519)),
    'F3' => parseCmap(objStream($byId, 509)),
    'F4' => parseCmap(objStream($byId, 514)),
];

function decodeHex(string $hex, array $map): string {
    $hex = preg_replace('/\s+/', '', $hex);
    $out = '';
    for ($i = 0; $i < strlen($hex); $i += 2) {
        $b = hexdec(substr($hex, $i, 2));
        $out .= $map[$b] ?? ('?' . sprintf('%02X', $b));
    }
    return $out;
}

$pages = [2, 68, 108, 131, 150, 173, 204, 222, 235, 261, 366, 490];
$all = '';
foreach ($pages as $n => $id) {
    $content = objStream($byId, $id);
    $font = 'F2';
    $pageText = '';
    // Walk tokens roughly: /Fx .. Tf and <hex> Tj and [(...)] TJ
    if (preg_match_all('/(\/F[1-4])\s+[0-9.]+\s+Tf|<([0-9A-Fa-f\s]+)>\s*Tj|\[(.*?)\]\s*TJ|\((?:\\\\.|[^\\\\)])*\)\s*Tj/s', $content, $ops, PREG_SET_ORDER)) {
        foreach ($ops as $op) {
            $rawOp = $op[0];
            if (preg_match('/\/(F[1-4])/', $rawOp, $fm) && str_contains($rawOp, 'Tf')) {
                $font = $fm[1];
                continue;
            }
            if (preg_match('/<([0-9A-Fa-f\s]+)>\s*Tj/', $rawOp, $hm)) {
                $pageText .= decodeHex($hm[1], $fonts[$font]) . "\n";
                continue;
            }
            if (preg_match('/\[(.*?)\]\s*TJ/s', $rawOp, $tm)) {
                if (preg_match_all('/<([0-9A-Fa-f\s]+)>/', $tm[1], $hs)) {
                    $line = '';
                    foreach ($hs[1] as $h) $line .= decodeHex($h, $fonts[$font]);
                    $pageText .= $line . "\n";
                }
            }
        }
    }
    $all .= "\n\n========== PAGE " . ($n+1) . " ==========\n" . $pageText;
}
file_put_contents(__DIR__ . '/tmp_pdf_text.txt', $all);
echo "written " . strlen($all) . "\n";
echo substr($all, 0, 6000);
