import React, {PropTypes, Component} from "react";
import {
    ArrowLeftIcon,
    ArrowRightIcon,
    ArrowEndLeftIcon,
    ArrowEndRightIcon
} from "dnn-svg-icons";
import DropDown from "dnn-dropdown";
import "./style.less";

class Pager extends Component {
    constructor(props) {
        super(props);
        this.state = {
            currentPage: 0,
            pageSize: props.pageSize,
            totalPages: 0,
            startIndex: 0,
            endIndex: props.numericCounters !== undefined ? props.numericCounters : 0
        };
        this.calculateTotalPages(props);
    }
    componentWillReceiveProps(newProps) {
        this.calculateTotalPages(newProps);
    }

    calculateTotalPages(props) {
        const {state} = this;
        if (state.pageSize >= props.totalRecords || props.totalRecords === 0) {
            state.totalPages = 1;
        } else {
            let totalPages = parseInt(props.totalRecords / state.pageSize);
            if (props.totalRecords % state.pageSize !== 0) {
                totalPages++;
            }
            state.totalPages = totalPages;
            let {endIndex} = state;
            if (endIndex > totalPages)
                state.endIndex = totalPages;
        }

        this.setState({
            state
        });
    }

    format() {
        let format = arguments[0];
        let methodsArgs = arguments;
        return format.replace(/{(\d+)}/gi, function (value, index) {
            let argsIndex = parseInt(index) + 1;
            return methodsArgs[argsIndex];
        });
    }
    onPageSizeSelected(option) {
        let {state} = this;
        if (state.pageSize !== option.value) {
            state.pageSize = option.value;
            this.setState({
                state
            });
            this.calculateTotalPages(this.props);
            this.onPageChanged(0);
        }
    }
    onPageChanged(step) {
        let {currentPage} = this.state;
        let {state} = this;
        switch (step) {
            case ">": {
                currentPage++;
                break;
            }
            case "<": {
                currentPage--;
                break;
            }
            case ">>": {
                currentPage = state.totalPages - 1;
                break;
            }
            case "<<": {
                currentPage = 0;
                break;
            }
            default: {
                currentPage = step;
                break;
            }
        }
        let {startIndex} = state;
        let {endIndex} = state;
        if (this.props.numericCounters > 0) {
            let boxesHalf = parseInt(this.props.numericCounters / 2);
            if (currentPage < startIndex && currentPage < state.totalPages && currentPage > 0) {
                startIndex = (startIndex - boxesHalf) < 0 ? 0 : startIndex - boxesHalf;
                endIndex = startIndex > 0 ? endIndex - boxesHalf : endIndex;
            }
            else if (currentPage >= endIndex - 1 && currentPage < (state.totalPages - 1) && currentPage > 0) {
                endIndex = (endIndex + boxesHalf) > state.totalPages ? state.totalPages : endIndex + boxesHalf;
                startIndex = endIndex < state.totalPages ? startIndex + boxesHalf :
                    ((endIndex - this.props.numericCounters) > 0 ? (endIndex - this.props.numericCounters) : 0);
            }
            else if (currentPage >= state.totalPages - 1) {
                endIndex = state.totalPages;
                startIndex = (endIndex - this.props.numericCounters) > 0 ? (endIndex - this.props.numericCounters) : 0;
            }
            else if (currentPage <= 0) {
                startIndex = 0;
                endIndex = (startIndex + this.props.numericCounters) > state.totalPages - 1 ? state.totalPages : (startIndex + this.props.numericCounters);
            }
        }
        this.setState({
            currentPage,
            startIndex,
            endIndex
        });
        this.props.onPageChanged(currentPage, state.pageSize);
    }

    getPageSummary() {
        if (this.props.showSummary) {
            const {props, state} = this;
            if (props.totalRecords <= 0) {
                return null;
            }
            let start = state.currentPage * state.pageSize + 1;
            let end = start + state.pageSize - 1;
            if (end > props.totalRecords) {
                end = props.totalRecords;
            }
            return this.format(this.props.summaryText, start, end, props.totalRecords);
        }
        return "&nbsp;";
    }
    getPagingBoxes() {
        let {state} = this;
        let {currentPage} = state;
        let {startIndex} = state;
        let {endIndex} = state;
        let pagingBoxes = [];
        if (this.props.numericCounters > 1) {
            for (let i = startIndex; i < endIndex; i++) {
                let step = i + 1;
                if (i !== currentPage) {
                    pagingBoxes = pagingBoxes.concat(<li className="pages do-not-close" onClick={this.onPageChanged.bind(this, i) }>{step}</li>);
                } else {
                    pagingBoxes = pagingBoxes.concat(<li className="pages current do-not-close"> {i + 1}</li>);
                }
            }
        }
        else {
            pagingBoxes = pagingBoxes.concat(<li className="pages current do-not-close"> {currentPage + 1}</li>);
        }
        return pagingBoxes;
    }
    getPageSizeDropDown() {
        let pageSizeOptions = [];
        if (this.props.totalRecords >= 10) pageSizeOptions.push({ "value": 10, "label": this.format(this.props.pageSizeOptionText, 10) });
        if (this.props.totalRecords >= 25) pageSizeOptions.push({ "value": 25, "label": this.format(this.props.pageSizeOptionText, 25) });
        if (this.props.totalRecords >= 50) pageSizeOptions.push({ "value": 50, "label": this.format(this.props.pageSizeOptionText, 50) });
        if (this.props.totalRecords >= 100) pageSizeOptions.push({ "value": 100, "label": this.format(this.props.pageSizeOptionText, 100) });
        if (this.props.totalRecords >= 250) pageSizeOptions.push({ "value": 250, "label": this.format(this.props.pageSizeOptionText, 250) });

        if (!pageSizeOptions.some(option => option.value === this.props.pageSize)) {
            pageSizeOptions =  pageSizeOptions.concat({ "value": this.props.pageSize, "label": this.format(this.props.pageSizeOptionText, this.props.pageSize) });
            pageSizeOptions = pageSizeOptions.sort(function (a, b) {
                let valueA = a.value;
                let valueB = b.value;
                if (valueA < valueB)
                    return -1;
                if (valueA > valueB)
                    return 1;
                return 0;
            });
        }

        return (<DropDown
            options={pageSizeOptions}
            value={this.props.pageSize }
            onSelect={this.onPageSizeSelected.bind(this) }
            withBorder={!this.props.pageSizeDropDownWithoutBorder}
            />
        );
    }
    /* eslint-disable react/no-danger */
    renderIcon(OnClick, Type, Disabled) {
        if (!Disabled) {
            return <li className={"do-not-close pages prev"} onClick={OnClick.bind(this) }>
                <span className="icon-button"  dangerouslySetInnerHTML={{ __html: Type }} />
            </li>;
        } else {
            return <li className={"do-not-close pages prev disabled"} >
                <span className="icon-flat"  dangerouslySetInnerHTML={{ __html: Type }} />
            </li>;
        }
    }
    render() {
        const {state, props} = this;
        return (state.totalPages > 1 || (state.totalPages === 1 && this.props.showPageSizeOptions)) &&
            <div className="dnn-pager do-not-close" style={props.style}>
                <div className="dnn-pager-summary-box">
                    {this.getPageSummary() }
                </div>
                <div className="dnn-pager-control">
                    <div className="dnn-pager-paging-box">
                        <ul>
                            {
                                this.props.showStartEndButtons &&
                                this.renderIcon(this.onPageChanged.bind(this, "<<"), ArrowEndLeftIcon, state.currentPage < 1)
                            }
                            {
                                this.renderIcon(this.onPageChanged.bind(this, "<"), ArrowLeftIcon, state.currentPage < 1)
                            }
                            {this.getPagingBoxes() }
                            {
                                this.renderIcon(this.onPageChanged.bind(this, ">"), ArrowRightIcon, state.totalPages <= (state.currentPage + 1))
                            }
                            {
                                this.props.showStartEndButtons &&
                                this.renderIcon(this.onPageChanged.bind(this, ">>"), ArrowEndRightIcon, state.totalPages <= (state.currentPage + 1))
                            }
                        </ul>
                    </div>
                    {this.props.showPageInfo && !this.props.showPageSizeOptions &&
                        <div className="dnn-pager-options-info-box">
                            {this.format(this.props.pageInfoText, state.currentPage + 1, state.totalPages) }
                        </div>
                    }
                    {
                        this.props.showPageSizeOptions &&
                        <div className="dnn-pager-pageSize-box">
                            {   this.getPageSizeDropDown() }
                        </div>
                    }
                </div>
            </div >;
    }
}
Pager.propTypes = {
    showStartEndButtons: PropTypes.bool,
    showPageSizeOptions: PropTypes.bool,
    pageSizeDropDownWithoutBorder: PropTypes.bool,
    showSummary: PropTypes.bool,
    showPageInfo: PropTypes.bool,
    summaryText: PropTypes.string,
    pageInfoText: PropTypes.string,
    numericCounters: PropTypes.number,
    pageSizeOptionText: PropTypes.string,
    pageSize: PropTypes.number,
    style: PropTypes.object,
    totalRecords: PropTypes.number.isRequired,
    onPageChanged: PropTypes.func.isRequired
};

Pager.defaultProps = {
    showStartEndButtons: true,
    showPageSizeOptions: true,
    pageSizeDropDownWithoutBorder: false,
    numericCounters: 1,
    showSummary: true,
    showPageInfo: false,
    summaryText: "Showing {0}-{1} of {2}",
    pageInfoText: "Page {0} of {1}",
    pageSizeOptionText: "{0} per page",
    pageSize: 10
};
/*
showPageSizeOptions and showPageInfo are mutually exclusive.
Preferred value for numericCounters is odd. 
*/

/*Sample onPageChanged method*/
// onPageChanged(currentPage, pageSize) {
//         let {state} = this;
//         if (pageSize !== undefined && state.pageSize !== pageSize) {
//             state.pageSize = pageSize;
//         }
//         state.currentPage = currentPage;
//         this.setState({
//             state
//         });
//         this.getData();
//     }

export default (Pager);