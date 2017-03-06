import React, { Component, PropTypes } from "react";
import debounce from "lodash/debounce";
import MagnifyingGlassIcon from "./MagnifyingGlassIcon";
import "./style.less";

const style = {
    main: {
        position: "relative",
        padding: 0,
        display: "inline-block"
    },
    input: {            
        position: "absolute",
        border: "none",
        backgroundColor: "transparent",
        color: "inherit",        
        paddingLeft: 20,
        width: "100%",
        height: "100%"
    },
    magnifyingGlass: {
        cursor: "pointer", 
        position: "absolute",
        width: 24,
        height: 24,
        right: 20,
        top: "50%",
        transform: "translateY(-50%)"
    }
};

class SearchBox extends Component {
    constructor() {
        super();
        this.state = {
            text: ""
        };
    }
    
    componentWillMount() {
        this.debouncedSearch = debounce(this.search, 1000);
    }
    
    search() {
        // TODO: do not search if no text, except if the text has been deleted
        // if (this.state.text.trim() === "") {
        //     return;
        // }
        
        this.props.onSearch(this.state.text);                
    }    
    
    onTextChanged(e) {
        this.setState({text: e.target.value}, () => {
            this.debouncedSearch();
        });     
    }
    
    render() {
        return (
            <div className={"dnn-search-box" + (this.props.className ? " " + this.props.className : "")} style={{...style.main, ...this.props.style}}>
                <input defaultValue={this.props.initialValue} style={{...style.input, ...this.props.inputStyle}} type="search" placeholder={this.props.placeholder} onChange={this.onTextChanged.bind(this)} aria-label="Search" />
                <MagnifyingGlassIcon style={{...style.magnifyingGlass, ...this.props.iconStyle}} onClick={this.search.bind(this)} />
            </div>
        );
    }
}

SearchBox.propTypes = {
    initialValue: PropTypes.string,
    style: PropTypes.object,
    inputStyle: PropTypes.object,
    iconStyle: PropTypes.object,
    className: PropTypes.string,
    onSearch: PropTypes.func.isRequired,
    placeholder: PropTypes.string
};

SearchBox.defaultProps = {
    placeholder: "Search"
};

export default SearchBox;