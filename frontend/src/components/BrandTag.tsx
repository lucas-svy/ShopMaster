/** Logo ShopMaster : une étiquette de prix. */
export function BrandTag({ size = 'small' }: { size?: 'small' | 'large' }) {
  return (
    <span className={`brand-tag brand-tag--${size}`}>
      <span className="brand-tag__hole" aria-hidden="true" />
      ShopMaster
    </span>
  );
}
