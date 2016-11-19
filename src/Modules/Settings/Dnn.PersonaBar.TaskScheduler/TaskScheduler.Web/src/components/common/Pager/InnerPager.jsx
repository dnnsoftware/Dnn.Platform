import React, {Component, PropTypes} from "react";
import PagesDescription from "./PagesDescription";
import Button from "./Button";
import { ArrowLeftIcon, ArrowRightIcon, ArrowEndLeftIcon, ArrowEndRightIcon } from "dnn-svg-icons";

const style = {
    float: "right",
    borderTop: "solid 1px #c8c8c8",
    borderLeft: "solid 1px #c8c8c8",
    borderBottom: "solid 1px #c8c8c8"
};

class InnerPager extends Component {
    isFirstPage() {
        return this.props.currentPage <= 1;
    }
    
    isLastPage() {
        return this.props.currentPage >= this.props.totalPages;
    }
    
    onPreviousPage() {
        if (!this.isFirstPage()) {                                        
            this.props.onPreviousPage();
        }
    }
    
    onNextPage() {
        if (!this.isLastPage()) {                            
            this.props.onNextPage();
        }
    }

    onFirstPage() {
        if (!this.isFirstPage()) {                            
            this.props.onFirstPage();
        }
    }

    onLastPage() {
        if (!this.isLastPage()) {                            
            this.props.onLastPage();
        }
    }
    
    render() {
        return (
            <div style={style}>                                   
                <Button onClick={this.onFirstPage.bind(this)} disabled={this.isFirstPage()} icon={ArrowEndLeftIcon} />    
                <Button onClick={this.onPreviousPage.bind(this)} disabled={this.isFirstPage()} icon={ArrowLeftIcon} />    
                <PagesDescription currentPage={this.props.currentPage} pagesDescriptionStyle={this.props.pagesDescriptionStyle}/>                     
                <Button onClick={this.onNextPage.bind(this)} disabled={this.isLastPage()} icon={ArrowRightIcon} />
                <Button onClick={this.onLastPage.bind(this)} disabled={this.isLastPage()} icon={ArrowEndRightIcon} />
            </div>
        );
    }
}

InnerPager.propTypes = {
    currentPage: PropTypes.number.isRequired,
    totalPages: PropTypes.number.isRequired,
    pageSize: PropTypes.number.isRequired,
    onPreviousPage: PropTypes.func.isRequired,
    onNextPage: PropTypes.func.isRequired,
    onFirstPage: PropTypes.func.isRequired,
    onLastPage: PropTypes.func.isRequired,
    pagesDescriptionStyle: PropTypes.object
};

export default InnerPager;