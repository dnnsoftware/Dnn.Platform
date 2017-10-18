"use strict";

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _react = require("react");

var _react2 = _interopRequireDefault(_react);

var _reactModal = require("react-modal");

var _reactModal2 = _interopRequireDefault(_reactModal);

var _reactCustomScrollbars = require("react-custom-scrollbars");

var _dnnSvgIcons = require("dnn-svg-icons");

require("./style.less");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var Modal = function (_Component) {
    _inherits(Modal, _Component);

    function Modal() {
        _classCallCheck(this, Modal);

        return _possibleConstructorReturn(this, (Modal.__proto__ || Object.getPrototypeOf(Modal)).apply(this, arguments));
    }

    _createClass(Modal, [{
        key: "getScrollbarStyle",
        value: function getScrollbarStyle(props) {
            return {
                width: "100%",
                height: props.header ? "calc(100% - 55px)" : "100%",
                boxSizing: "border-box",
                padding: "25px 30px"
            };
        }
    }, {
        key: "getModalStyles",
        value: function getModalStyles(props) {
            var modalWidth = props.modalWidth;
            var modalTopMargin = props.modalTopMargin;
            if (document.getElementsByClassName("socialpanel") && document.getElementsByClassName("socialpanel").length > 0 && !props.modalWidth) {
                modalWidth = document.getElementsByClassName("socialpanel")[0].offsetWidth;
            }
            if (document.getElementsByClassName("dnn-persona-bar-page-header") && document.getElementsByClassName("dnn-persona-bar-page-header").length > 0 && !props.modalHeight) {
                modalTopMargin = document.getElementsByClassName("dnn-persona-bar-page-header")[0].offsetHeight;
            }
            return props.style || {
                overlay: {
                    zIndex: "99999",
                    backgroundColor: "rgba(0,0,0,0.6)"
                },
                content: {
                    top: modalTopMargin + props.dialogVerticalMargin,
                    left: props.dialogHorizontalMargin + 85,
                    padding: 0,
                    borderRadius: 0,
                    border: "none",
                    width: modalWidth - props.dialogHorizontalMargin * 2,
                    height: props.modalHeight || "60%",
                    backgroundColor: "#FFFFFF",
                    position: "absolute",
                    userSelect: "none",
                    WebkitUserSelect: "none",
                    MozUserSelect: "none",
                    MsUserSelect: "none",
                    boxSizing: "border-box"
                }
            };
        }
        /*eslint-disable react/no-danger*/

    }, {
        key: "render",
        value: function render() {
            var props = this.props;

            var modalStyles = this.getModalStyles(props);
            var scrollBarStyle = this.getScrollbarStyle(props);
            return _react2.default.createElement(
                _reactModal2.default,
                {
                    isOpen: props.isOpen,
                    onRequestClose: props.onRequestClose,
                    onAfterOpen: props.onAfterOpen,
                    closeTimeoutMS: props.closeTimeoutMS,
                    shouldCloseOnOverlayClick: props.shouldCloseOnOverlayClick,
                    style: modalStyles },
                props.header && _react2.default.createElement(
                    "div",
                    { className: "modal-header" },
                    _react2.default.createElement(
                        "h3",
                        null,
                        props.header
                    ),
                    props.headerChildren,
                    _react2.default.createElement("div", {
                        className: "close-modal-button",
                        dangerouslySetInnerHTML: { __html: _dnnSvgIcons.XThinIcon },
                        onClick: props.onRequestClose })
                ),
                _react2.default.createElement(
                    _reactCustomScrollbars.Scrollbars,
                    { style: scrollBarStyle },
                    _react2.default.createElement(
                        "div",
                        { style: props.contentStyle },
                        props.children
                    )
                )
            );
        }
    }]);

    return Modal;
}(_react.Component);

Modal.propTypes = {
    isOpen: _react.PropTypes.bool,
    style: _react.PropTypes.object,
    onRequestClose: _react.PropTypes.func,
    children: _react.PropTypes.node,
    dialogVerticalMargin: _react.PropTypes.number,
    dialogHorizontalMargin: _react.PropTypes.number,
    modalWidth: _react.PropTypes.number,
    modalHeight: _react.PropTypes.number,
    modalTopMargin: _react.PropTypes.number,
    header: _react.PropTypes.string,
    headerChildren: _react.PropTypes.node,
    contentStyle: _react.PropTypes.object,
    onAfterOpen: _react.PropTypes.func,
    closeTimeoutMS: _react.PropTypes.number,
    shouldCloseOnOverlayClick: _react.PropTypes.bool
};
Modal.defaultProps = {
    modalWidth: 861,
    modalTopMargin: 100,
    dialogVerticalMargin: 25,
    dialogHorizontalMargin: 30,
    contentStyle: { padding: "25px 30px" },
    shouldCloseOnOverlayClick: true
};
exports.default = Modal;
//# sourceMappingURL=Modal.js.map