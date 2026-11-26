<?php
/** @var string $title */
/** @var int $code */
/** @var string $message */
?>
<div class="row justify-content-center py-5">
    <div class="col-12 col-md-8 col-lg-6 text-center">
        <div class="card border-0 shadow-sm p-4 p-md-5" style="border-radius: var(--dl-radius-card);">
            <div class="display-1 fw-bold text-danger mb-3"><?= (int) $code ?></div>
            <h1 class="h3 fw-bold mb-3"><?= $this->e($title) ?></h1>
            <p class="text-muted mb-4"><?= nl2br($this->e($message)) ?></p>
            <div class="d-flex justify-content-center gap-2">
                <a href="<?= $this->url('/doctors') ?>" class="btn btn-primary rounded-pill px-4">В каталог врачей</a>
                <a href="<?= $this->url('/') ?>" class="btn btn-outline-secondary rounded-pill px-4">На главную</a>
            </div>
        </div>
    </div>
</div>
