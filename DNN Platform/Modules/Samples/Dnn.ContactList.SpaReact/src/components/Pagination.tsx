interface PaginationProps {
  currentPage: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

export default function Pagination({ currentPage, totalItems, pageSize, onPageChange }: PaginationProps) {
  const totalPages = Math.ceil(totalItems / pageSize);
  const startItem = totalItems === 0 ? 0 : currentPage * pageSize + 1;
  const endItem = Math.min((currentPage + 1) * pageSize, totalItems);

  const handlePrev = () => {
    if (currentPage > 0) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages - 1) {
      onPageChange(currentPage + 1);
    }
  };

  if (totalItems === 0) {
    return null;
  }

  return (
    <div className="contactList-pagination">
      <div>
        Showing {startItem}-{endItem} of {totalItems}
      </div>
      <a
        onClick={handlePrev}
        className={`pagination-btn ${currentPage === 0 ? 'disabled' : ''}`}
        style={{ cursor: currentPage === 0 ? 'not-allowed' : 'pointer', opacity: currentPage === 0 ? 0.5 : 1 }}
      >
        <i className="fa fa-arrow-left"></i>
      </a>
      <a
        onClick={handleNext}
        className={`pagination-btn ${currentPage >= totalPages - 1 ? 'disabled' : ''}`}
        style={{ cursor: currentPage >= totalPages - 1 ? 'not-allowed' : 'pointer', opacity: currentPage >= totalPages - 1 ? 0.5 : 1 }}
      >
        <i className="fa fa-arrow-right"></i>
      </a>
      <div>
        Page {currentPage + 1} of {totalPages}
      </div>
    </div>
  );
}

