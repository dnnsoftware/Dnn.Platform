import React, { Component, PropTypes } from "react";
import ItemsDescription from "./ItemsDescription";
import InnerPager from "./InnerPager";
import PageSize from "./PageSize";

class Pager extends Component {
    getFrom() {
        return parseInt(this.props.startIndex) + 1;
    }

    getTo() {
        return Math.min(this.getFrom() + parseInt(this.props.pageSize) - 1, parseInt(this.props.total));
    }

    getTotalPages() {
        let total = Math.ceil(parseInt(this.props.total) / parseInt(this.props.pageSize));
        return total === 0 ? 1 : total;
    }

    getCurrentPage() {
        return parseInt(this.props.startIndex) / parseInt(this.props.pageSize) + 1;
    }

    previousPage() {
        const startIndex = parseInt(this.props.startIndex) - parseInt(this.props.pageSize);
        this.props.onPageChange(startIndex);
    }

    nextPage() {
        const startIndex = parseInt(this.props.startIndex) + parseInt(this.props.pageSize);
        this.props.onPageChange(startIndex);
    }

    firstPage() {
        this.props.onPageChange(0);
    }

    lastPage() {
        const startIndex = (this.getTotalPages() - 1) * parseInt(this.props.pageSize);
        this.props.onPageChange(startIndex);
    }

    render() {
        return (
            <div className={this.props.className}>
                <ItemsDescription {...this.props} from={this.getFrom() } to={this.getTo() } />
                <InnerPager
                    currentPage={this.getCurrentPage() }
                    totalPages={this.getTotalPages() }
                    pageSize={this.props.pageSize}
                    onPreviousPage={this.previousPage.bind(this) }
                    onNextPage={this.nextPage.bind(this) }
                    onFirstPage={this.firstPage.bind(this) }
                    onLastPage={this.lastPage.bind(this) }/>
                <PageSize {...this.props} />
            </div>
        );
    }
}

Pager.propTypes = {
    total: PropTypes.number.isRequired,
    startIndex: PropTypes.number,
    pageSize: PropTypes.number,
    className: PropTypes.string,
    onPageChange: PropTypes.func.isRequired,
    onPageSizeChange: PropTypes.func.isRequired,
    pageSizeOptions: PropTypes.array.isRequired,
    pageSizeStyle: PropTypes.object.isRequired
};

Pager.defaultProps = {
    startIndex: 0,
    pageSize: 10
};

export default Pager;