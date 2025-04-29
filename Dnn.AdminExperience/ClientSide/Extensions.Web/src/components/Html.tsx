import parse from "html-react-parser";
import DOMPurify from "dompurify";

export default function Html({ html }) {
  return parse(DOMPurify.sanitize(html));
}
